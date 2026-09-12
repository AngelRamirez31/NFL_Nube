using Moq;
using TournamentServices.Delegates;
using TournamentServices.Domain;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Delegates.Tests;

// ============================================================
// PERSONA 1 — Ejemplo de cómo mockear el repositorio con Moq.
// Persona 2/3/4: copien este patrón para Tournament/Group/MatchDelegateTests.
// Estos tests fallarán mientras TeamDelegate siga lanzando
// NotImplementedException — eso es esperado hasta que lo implementen.
// ============================================================
public class TeamDelegateTests
{
    private readonly Mock<ITeamRepository> _repoMock = new();
    private readonly TeamDelegate _delegate;

    public TeamDelegateTests()
    {
        _delegate = new TeamDelegate(_repoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsTeam()
    {
        var team = new Team { Id = "team-1", Name = "Eagles" };
        _repoMock.Setup(r => r.GetByIdAsync("team-1")).ReturnsAsync(team);

        var result = await _delegate.GetByIdAsync("team-1");

        Assert.Equal(team, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Team?)null);

        var result = await _delegate.GetByIdAsync("missing");

        Assert.Null(result);
    }

    // TODO: PERSONA 1 — completar según la tabla "Required Test Cases per Delegate"
    // del contrato: GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync.
}
