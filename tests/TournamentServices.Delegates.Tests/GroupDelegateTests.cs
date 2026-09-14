using Moq;
using TournamentServices.Delegates;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Delegates.Tests;

public class GroupDelegateTests
{
    private const string TournamentId = "tournament-1";
    private const string GroupId = "group-1";

    private readonly Mock<IGroupRepository> _groupRepository = new();
    private readonly Mock<ITeamRepository> _teamRepository = new();
    private readonly Mock<ITournamentRepository> _tournamentRepository = new();

    private readonly GroupDelegate _delegate;

    public GroupDelegateTests()
    {
        _delegate = new GroupDelegate(
            _groupRepository.Object,
            _teamRepository.Object,
            _tournamentRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGroupDoesNotExist_ReturnsNotFound()
    {
        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync((Group?)null);

        var result = await _delegate.GetByIdAsync(
            TournamentId,
            GroupId);

        Assert.Equal(ErrorKind.NotFound, result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenTournamentDoesNotExist_ReturnsNotFound()
    {
        _tournamentRepository
            .Setup(r => r.GetByIdAsync(TournamentId))
            .ReturnsAsync((Tournament?)null);

        var result = await _delegate.CreateAsync(
            TournamentId,
            "AFC West");

        Assert.Equal(ErrorKind.NotFound, result.Error);

        _groupRepository.Verify(
            r => r.CreateAsync(It.IsAny<Group>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ReturnsConflict()
    {
        var tournament = CreateTournament();

        _tournamentRepository
            .Setup(r => r.GetByIdAsync(TournamentId))
            .ReturnsAsync(tournament);

        _groupRepository
            .Setup(r => r.ExistsByNameInTournamentAsync(
                TournamentId,
                "AFC West"))
            .ReturnsAsync(true);

        var result = await _delegate.CreateAsync(
            TournamentId,
            "AFC West");

        Assert.Equal(ErrorKind.Conflict, result.Error);

        _groupRepository.Verify(
            r => r.CreateAsync(It.IsAny<Group>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignTeamsAsync_WhenGroupDoesNotExist_ReturnsNotFound()
    {
        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync((Group?)null);

        var result = await _delegate.AssignTeamsAsync(
            TournamentId,
            GroupId,
            new[] { "team-1" });

        Assert.Equal(ErrorKind.NotFound, result.Error);
    }

    [Fact]
    public async Task AssignTeamsAsync_WhenRequestContainsDuplicateTeam_ReturnsConflict()
    {
        var group = CreateGroup();

        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync(group);

        var result = await _delegate.AssignTeamsAsync(
            TournamentId,
            GroupId,
            new[] { "team-1", "team-1" });

        Assert.Equal(ErrorKind.Conflict, result.Error);

        _teamRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignTeamsAsync_WhenCapacityWouldBeExceeded_ReturnsConflict()
    {
        var group = CreateGroup();

        group.Teams.AddRange(new[]
        {
            new Team { Id = "existing-1", Name = "One" },
            new Team { Id = "existing-2", Name = "Two" },
            new Team { Id = "existing-3", Name = "Three" }
        });

        var tournament = CreateTournament(maxTeamsPerGroup: 4);

        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync(group);

        _tournamentRepository
            .Setup(r => r.GetByIdAsync(TournamentId))
            .ReturnsAsync(tournament);

        var result = await _delegate.AssignTeamsAsync(
            TournamentId,
            GroupId,
            new[] { "team-1", "team-2" });

        Assert.Equal(ErrorKind.Conflict, result.Error);

        _teamRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignTeamsAsync_WhenTeamDoesNotExist_ReturnsConflict()
    {
        var group = CreateGroup();
        var tournament = CreateTournament();

        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync(group);

        _tournamentRepository
            .Setup(r => r.GetByIdAsync(TournamentId))
            .ReturnsAsync(tournament);

        _teamRepository
            .Setup(r => r.GetByIdAsync("missing-team"))
            .ReturnsAsync((Team?)null);

        var result = await _delegate.AssignTeamsAsync(
            TournamentId,
            GroupId,
            new[] { "missing-team" });

        Assert.Equal(ErrorKind.Conflict, result.Error);
    }

    [Fact]
    public async Task AssignTeamsAsync_WhenTeamBelongsToAnotherGroup_ReturnsConflict()
    {
        var group = CreateGroup();
        var tournament = CreateTournament();

        var team = new Team
        {
            Id = "team-1",
            Name = "Eagles"
        };

        var otherGroup = new Group
        {
            Id = "group-2",
            Name = "Other Group",
            TournamentId = TournamentId,
            Teams = new List<Team> { team }
        };

        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync(group);

        _tournamentRepository
            .Setup(r => r.GetByIdAsync(TournamentId))
            .ReturnsAsync(tournament);

        _teamRepository
            .Setup(r => r.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        _groupRepository
            .Setup(r => r.FindByTournamentAndTeamAsync(
                TournamentId,
                team.Id))
            .ReturnsAsync(otherGroup);

        var result = await _delegate.AssignTeamsAsync(
            TournamentId,
            GroupId,
            new[] { team.Id });

        Assert.Equal(ErrorKind.Conflict, result.Error);

        _groupRepository.Verify(
            r => r.UpdateAsync(
                It.IsAny<string>(),
                It.IsAny<Group>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignTeamsAsync_WhenValid_UpdatesGroupAndReturnsOk()
    {
        var group = CreateGroup();
        var tournament = CreateTournament();

        var team1 = new Team
        {
            Id = "team-1",
            Name = "Eagles"
        };

        var team2 = new Team
        {
            Id = "team-2",
            Name = "Cowboys"
        };

        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync(group);

        _tournamentRepository
            .Setup(r => r.GetByIdAsync(TournamentId))
            .ReturnsAsync(tournament);

        _teamRepository
            .Setup(r => r.GetByIdAsync(team1.Id))
            .ReturnsAsync(team1);

        _teamRepository
            .Setup(r => r.GetByIdAsync(team2.Id))
            .ReturnsAsync(team2);

        _groupRepository
            .Setup(r => r.FindByTournamentAndTeamAsync(
                TournamentId,
                It.IsAny<string>()))
            .ReturnsAsync((Group?)null);

        _groupRepository
            .Setup(r => r.UpdateAsync(GroupId, group))
            .ReturnsAsync(group);

        var result = await _delegate.AssignTeamsAsync(
            TournamentId,
            GroupId,
            new[] { team1.Id, team2.Id });

        Assert.Equal(ErrorKind.None, result.Error);
        Assert.Equal(2, group.Teams.Count);
        Assert.Contains(group.Teams, t => t.Id == team1.Id);
        Assert.Contains(group.Teams, t => t.Id == team2.Id);

        _groupRepository.Verify(
            r => r.UpdateAsync(GroupId, group),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenGroupExists_ReturnsOk()
    {
        var group = CreateGroup();

        _groupRepository
            .Setup(r => r.GetByIdAsync(TournamentId, GroupId))
            .ReturnsAsync(group);

        _groupRepository
            .Setup(r => r.DeleteAsync(GroupId))
            .ReturnsAsync(true);

        var result = await _delegate.DeleteAsync(
            TournamentId,
            GroupId);

        Assert.Equal(ErrorKind.None, result.Error);
    }

    private static Group CreateGroup()
    {
        return new Group
        {
            Id = GroupId,
            Name = "AFC West",
            TournamentId = TournamentId,
            Teams = new List<Team>()
        };
    }

    private static Tournament CreateTournament(
        int maxTeamsPerGroup = 4)
    {
        return new Tournament
        {
            Id = TournamentId,
            Name = "NFL Test",
            Format = new TournamentFormat
            {
                NumberOfGroups = 2,
                MaxTeamsPerGroup = maxTeamsPerGroup
            }
        };
    }
}