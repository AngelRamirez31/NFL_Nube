using Moq;
using TournamentServices.Delegates;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Domain.Enums;
using TournamentServices.Repositories;
using Xunit;
using Match = TournamentServices.Domain.Match;  

namespace TournamentServices.Delegates.Tests;

public class TournamentDelegateTests
{
    private const string TournamentId = "tournament-1";

    private readonly Mock<ITournamentRepository> _tournamentRepository = new();
    private readonly Mock<IGroupRepository> _groupRepository = new();
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly TournamentDelegate _delegate;

    public TournamentDelegateTests()
    {
        _delegate = new TournamentDelegate(
            _tournamentRepository.Object, _groupRepository.Object, _matchRepository.Object);

        _groupRepository.Setup(r => r.GetByTournamentAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<Group>());
        _matchRepository.Setup(r => r.GetByTournamentAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<Match>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        _tournamentRepository.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Tournament?)null);

        var result = await _delegate.GetByIdAsync("missing");

        Assert.Equal(ErrorKind.NotFound, result.Error);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidFormat_ReturnsInvalid()
    {
        var tournament = NewTournament();
        tournament.Format.NumberOfGroups = 0;

        var result = await _delegate.CreateAsync(tournament);

        Assert.Equal(ErrorKind.Validation, result.Error);
        _tournamentRepository.Verify(r => r.CreateAsync(It.IsAny<Tournament>()), Times.Never);
    }

    // Los campos que no vienen en el patch no se modifican.
    [Fact]
    public async Task PatchAsync_WithNameOnly_KeepsFormat()
    {
        GivenTournamentExists(NewTournament());

        var result = await _delegate.PatchAsync(TournamentId, "Nombre nuevo", null, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nombre nuevo", result.Value!.Name);
        Assert.Equal(2, result.Value.Format.NumberOfGroups);
        Assert.Equal(4, result.Value.Format.MaxTeamsPerGroup);
    }

    [Fact]
    public async Task PatchAsync_WithInvalidType_ReturnsInvalid()
    {
        GivenTournamentExists(NewTournament());

        var result = await _delegate.PatchAsync(TournamentId, null, null, null, "ROUND_ROBIN");

        Assert.Equal(ErrorKind.Validation, result.Error);
    }

    // Antes de borrar el torneo se borran sus partidos y grupos.
    [Fact]
    public async Task DeleteAsync_RemovesMatchesAndGroupsBeforeTheTournament()
    {
        GivenTournamentExists(NewTournament());
        _groupRepository.Setup(r => r.GetByTournamentAsync(TournamentId))
            .ReturnsAsync(new[] { new Group { Id = "group-1" } });
        _matchRepository.Setup(r => r.GetByTournamentAsync(TournamentId))
            .ReturnsAsync(new[] { new Match { Id = "match-1" } });
        _tournamentRepository.Setup(r => r.DeleteAsync(TournamentId)).ReturnsAsync(true);

        var result = await _delegate.DeleteAsync(TournamentId);

        Assert.True(result.IsSuccess);
        _matchRepository.Verify(r => r.DeleteAsync("match-1"), Times.Once);
        _groupRepository.Verify(r => r.DeleteAsync("group-1"), Times.Once);
    }

    // ---------- GenerateMatchesAsync ----------

    [Fact]
    public async Task GenerateMatchesAsync_WhenTournamentDoesNotExist_ReturnsNotFound()
    {
        _tournamentRepository.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Tournament?)null);

        var result = await _delegate.GenerateMatchesAsync("missing");

        Assert.Equal(ErrorKind.NotFound, result.Error);
    }

    // 2 grupos de 3 equipos: 3 partidos por grupo = 6 partidos.
    [Fact]
    public async Task GenerateMatchesAsync_FirstTime_CreatesAllVsAllInEachGroup()
    {
        GivenTournamentExists(NewTournament());
        _groupRepository.Setup(r => r.GetByTournamentAsync(TournamentId)).ReturnsAsync(new[]
        {
            NewGroup("group-a", "a1", "a2", "a3"),
            NewGroup("group-b", "b1", "b2", "b3")
        });
        _matchRepository.Setup(r => r.CreateAsync(It.IsAny<Match>())).ReturnsAsync((Match m) => m);

        var result = await _delegate.GenerateMatchesAsync(TournamentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value!.Count);
        Assert.All(result.Value, m => Assert.Equal(MatchStage.GROUP, m.Stage));
        _matchRepository.Verify(r => r.CreateAsync(It.IsAny<Match>()), Times.Exactly(6));
    }

    [Fact]
    public async Task GenerateMatchesAsync_WithMatchesWithoutScore_ReturnsConflict()
    {
        GivenTournamentExists(NewTournament());
        _matchRepository.Setup(r => r.GetByTournamentAsync(TournamentId))
            .ReturnsAsync(new[] { new Match { Id = "match-1", Score = null } });

        var result = await _delegate.GenerateMatchesAsync(TournamentId);

        Assert.Equal(ErrorKind.Conflict, result.Error);
        _matchRepository.Verify(r => r.CreateAsync(It.IsAny<Match>()), Times.Never);
    }

    [Fact]
    public async Task GenerateMatchesAsync_WhenAGroupHasLessThanTwoTeams_ReturnsConflict()
    {
        GivenTournamentExists(NewTournament());
        _groupRepository.Setup(r => r.GetByTournamentAsync(TournamentId)).ReturnsAsync(new[]
        {
            NewGroup("group-a", "a1", "a2"),
            NewGroup("group-b", "b1")
        });

        var result = await _delegate.GenerateMatchesAsync(TournamentId);

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    // El formato dice 2 grupos, pero solo hay 1.
    [Fact]
    public async Task GenerateMatchesAsync_WhenGroupsDoNotMatchTheFormat_ReturnsConflict()
    {
        GivenTournamentExists(NewTournament());
        _groupRepository.Setup(r => r.GetByTournamentAsync(TournamentId))
            .ReturnsAsync(new[] { NewGroup("group-a", "a1", "a2") });

        var result = await _delegate.GenerateMatchesAsync(TournamentId);

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    // ---------- GetChampionAsync ----------

    [Fact]
    public async Task GetChampionAsync_BeforeTheFinal_ReturnsConflict()
    {
        GivenTournamentExists(NewTournament());

        var result = await _delegate.GetChampionAsync(TournamentId);

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    [Fact]
    public async Task GetChampionAsync_WhenTournamentDoesNotExist_ReturnsNotFound()
    {
        _tournamentRepository.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Tournament?)null);

        var result = await _delegate.GetChampionAsync("missing");

        Assert.Equal(ErrorKind.NotFound, result.Error);
    }

    // ---------- GetStandingsAsync ----------

    [Fact]
    public async Task GetStandingsAsync_ReturnsOneTablePerGroup()
    {
        GivenTournamentExists(NewTournament());
        _groupRepository.Setup(r => r.GetByTournamentAsync(TournamentId)).ReturnsAsync(new[]
        {
            NewGroup("group-a", "a1", "a2"),
            NewGroup("group-b", "b1", "b2")
        });

        var result = await _delegate.GetStandingsAsync(TournamentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(2, result.Value[0].Standings.Count);
    }

    private static Group NewGroup(string id, params string[] teamIds) => new()
    {
        Id = id,
        Name = id,
        TournamentId = TournamentId,
        Teams = teamIds.Select(t => new Team { Id = t, Name = t }).ToList()
    };

    private static Tournament NewTournament() => new()
    {
        Id = TournamentId,
        Name = "NFL 2026",
        Format = new TournamentFormat { NumberOfGroups = 2, MaxTeamsPerGroup = 4, Type = TournamentType.NFL }
    };

    private void GivenTournamentExists(Tournament tournament)
    {
        _tournamentRepository.Setup(r => r.GetByIdAsync(tournament.Id)).ReturnsAsync(tournament);
        _tournamentRepository.Setup(r => r.UpdateAsync(tournament.Id, It.IsAny<Tournament>()))
            .ReturnsAsync((string _, Tournament t) => t);
    }
}
