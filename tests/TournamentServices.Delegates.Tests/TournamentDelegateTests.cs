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

    // READ-MODIFY-WRITE: lo que no viene en el patch se conserva.
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

    // GROUPS tiene FK a TOURNAMENTS: hay que borrar matches y groups antes.
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
