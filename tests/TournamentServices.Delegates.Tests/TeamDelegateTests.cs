using Moq;
using TournamentServices.Delegates;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Delegates.Tests;

public class TeamDelegateTests
{
    private readonly Mock<ITeamRepository> _repoMock = new();
    private readonly TeamDelegate _delegate;

    public TeamDelegateTests()
    {
        _delegate = new TeamDelegate(_repoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsOkWithTeam()
    {
        var team = new Team { Id = "team-1", Name = "Eagles" };
        _repoMock.Setup(r => r.GetByIdAsync("team-1")).ReturnsAsync(team);

        var result = await _delegate.GetByIdAsync("team-1");

        Assert.Equal(ErrorKind.None, result.Error);
        Assert.Equal(team, result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Team?)null);

        var result = await _delegate.GetByIdAsync("missing");

        Assert.Equal(ErrorKind.NotFound, result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ReturnsInvalid()
    {
        _repoMock.Setup(r => r.ExistsByNameAsync("Eagles")).ReturnsAsync(true);

        var result = await _delegate.CreateAsync(new Team { Name = "Eagles" });

        Assert.Equal(ErrorKind.Validation, result.Error);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Team>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ReturnsInvalid()
    {
        var result = await _delegate.CreateAsync(new Team { Name = "" });

        Assert.Equal(ErrorKind.Validation, result.Error);
    }

}
