using Moq;
using TournamentServices.Delegates;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Domain.Enums;
using TournamentServices.Repositories;
using Xunit;
using Match = TournamentServices.Domain.Match;   // desambigua contra Moq.Match

namespace TournamentServices.Delegates.Tests;

public class MatchDelegateTests
{
    private const string TournamentId = "tournament-1";
    private const string GroupId = "group-1";

    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<ITeamRepository> _teamRepository = new();
    private readonly Mock<IGroupRepository> _groupRepository = new();
    private readonly MatchDelegate _delegate;

    public MatchDelegateTests()
    {
        _delegate = new MatchDelegate(_matchRepository.Object, _teamRepository.Object, _groupRepository.Object);

        // Escenario feliz por defecto: dos equipos que existen y comparten grupo.
        GivenTeamExists("team-1", "Eagles");
        GivenTeamExists("team-2", "Cowboys");

        var group = new Group
        {
            Id = GroupId,
            TournamentId = TournamentId,
            Teams = { new Team { Id = "team-1" }, new Team { Id = "team-2" } }
        };
        _groupRepository.Setup(r => r.GetByIdAsync(TournamentId, GroupId)).ReturnsAsync(group);
        _groupRepository.Setup(r => r.FindByTournamentAndTeamAsync(TournamentId, "team-1")).ReturnsAsync(group);
        _groupRepository.Setup(r => r.FindByTournamentAndTeamAsync(TournamentId, "team-2")).ReturnsAsync(group);

        _matchRepository
            .Setup(r => r.CreateAsync(It.IsAny<Match>()))
            .ReturnsAsync((Match m) => { m.Id = "match-1"; return m; });
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_WithValidTeams_ReturnsOkAndPersists()
    {
        var result = await _delegate.CreateAsync(TournamentId, GroupId, "team-1", "team-2");

        Assert.True(result.IsSuccess);
        Assert.Equal("match-1", result.Value!.Id);
        Assert.Null(result.Value.Score);          // aún no se juega
        Assert.False(result.Value.IsCompleted);
        _matchRepository.Verify(r => r.CreateAsync(It.IsAny<Match>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithSameTeamOnBothSides_ReturnsConflict()
    {
        var result = await _delegate.CreateAsync(TournamentId, GroupId, "team-1", "team-1");

        Assert.Equal(ErrorKind.Conflict, result.Error);   // -> 422
        _matchRepository.Verify(r => r.CreateAsync(It.IsAny<Match>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenHomeTeamDoesNotExist_ReturnsConflict()
    {
        _teamRepository.Setup(r => r.GetByIdAsync("ghost")).ReturnsAsync((Team?)null);

        var result = await _delegate.CreateAsync(TournamentId, GroupId, "ghost", "team-2");

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenTeamIsNotInTheTournament_ReturnsConflict()
    {
        // team-3 existe, pero FindByTournamentAndTeamAsync no lo encuentra en ningún grupo.
        GivenTeamExists("team-3", "Giants");

        var result = await _delegate.CreateAsync(TournamentId, groupId: null, "team-1", "team-3");

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenGroupIsNotInTheTournament_ReturnsConflict()
    {
        var result = await _delegate.CreateAsync(TournamentId, "group-desconocido", "team-1", "team-2");

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenTeamsAreNotInTheSameGroup_ReturnsConflict()
    {
        GivenTeamExists("team-3", "Giants");
        // group-1 solo tiene team-1 y team-2 (setup del constructor); team-3
        // no pertenece a ese grupo aunque exista en el torneo.
        var result = await _delegate.CreateAsync(TournamentId, GroupId, "team-1", "team-3");

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    [Fact]
    public async Task CreateAsync_WithoutGroupId_OnlyRequiresTeamsInTheTournament()
    {
        var result = await _delegate.CreateAsync(TournamentId, groupId: null, "team-1", "team-2");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.GroupId);
    }

    // ---------- UpdateScoreAsync ----------

    [Fact]
    public async Task UpdateScoreAsync_ComputesWinnerAndMarksCompleted()
    {
        GivenMatchExists(new Match { Id = "match-1", TournamentId = TournamentId });

        var result = await _delegate.UpdateScoreAsync(TournamentId, "match-1", 31, 28);

        Assert.True(result.IsSuccess);
        Assert.Equal(Winner.HOME, result.Value!.Winner);
        Assert.True(result.Value.IsCompleted);
    }

    [Fact]
    public async Task UpdateScoreAsync_WhenVisitorWins_ReturnsVisitor()
    {
        GivenMatchExists(new Match { Id = "match-1", TournamentId = TournamentId });

        var result = await _delegate.UpdateScoreAsync(TournamentId, "match-1", 7, 35);

        Assert.Equal(Winner.VISITOR, result.Value!.Winner);
    }

    // Empate = HOME (pendiente de confirmar con el profesor, ver Domain/Score.cs).
    [Fact]
    public async Task UpdateScoreAsync_WhenTied_ReturnsHomeAndIsCompleted()
    {
        GivenMatchExists(new Match { Id = "match-1", TournamentId = TournamentId });

        var result = await _delegate.UpdateScoreAsync(TournamentId, "match-1", 20, 20);

        Assert.Equal(Winner.HOME, result.Value!.Winner);
        Assert.True(result.Value.IsCompleted);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -7)]
    [InlineData(-3, -3)]
    public async Task UpdateScoreAsync_WithNegativeScore_ReturnsValidation(int home, int visitor)
    {
        var result = await _delegate.UpdateScoreAsync(TournamentId, "match-1", home, visitor);

        Assert.Equal(ErrorKind.Validation, result.Error);   // -> 400, no 422
        _matchRepository.Verify(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Match>()), Times.Never);
    }

    [Fact]
    public async Task UpdateScoreAsync_WhenMatchDoesNotExist_ReturnsNotFound()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(TournamentId, "missing")).ReturnsAsync((Match?)null);

        var result = await _delegate.UpdateScoreAsync(TournamentId, "missing", 10, 3);

        Assert.Equal(ErrorKind.NotFound, result.Error);   // -> 404
    }

    // ---------- GetByIdAsync / DeleteAsync ----------

    [Fact]
    public async Task GetByIdAsync_HydratesTeamDetails()
    {
        GivenMatchExists(new Match
        {
            Id = "match-1",
            TournamentId = TournamentId,
            HomeTeamId = "team-1",
            VisitorTeamId = "team-2"
        });

        var result = await _delegate.GetByIdAsync(TournamentId, "match-1");

        Assert.True(result.IsSuccess);
        Assert.Equal("Eagles", result.Value!.HomeTeam!.Name);
        Assert.Equal("Cowboys", result.Value.VisitorTeam!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNotFound()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(TournamentId, "missing")).ReturnsAsync((Match?)null);

        var result = await _delegate.GetByIdAsync(TournamentId, "missing");

        Assert.Equal(ErrorKind.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenMatchExists_ReturnsOk()
    {
        GivenMatchExists(new Match { Id = "match-1", TournamentId = TournamentId });
        _matchRepository.Setup(r => r.DeleteAsync("match-1")).ReturnsAsync(true);

        var result = await _delegate.DeleteAsync(TournamentId, "match-1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_WhenMatchDoesNotExist_ReturnsNotFound()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(TournamentId, "missing")).ReturnsAsync((Match?)null);

        var result = await _delegate.DeleteAsync(TournamentId, "missing");

        Assert.Equal(ErrorKind.NotFound, result.Error);
        _matchRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    // ---------- helpers ----------

    private void GivenTeamExists(string id, string name) =>
        _teamRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Team { Id = id, Name = name });

    private void GivenMatchExists(Match match)
    {
        _matchRepository.Setup(r => r.GetByIdAsync(match.TournamentId, match.Id)).ReturnsAsync(match);
        _matchRepository.Setup(r => r.UpdateAsync(match.Id, It.IsAny<Match>()))
            .ReturnsAsync((string _, Match m) => m);
    }
}
