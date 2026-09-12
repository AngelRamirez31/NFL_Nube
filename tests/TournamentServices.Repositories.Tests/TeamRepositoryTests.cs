using TournamentServices.Domain;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Repositories.Tests;

// PERSONA 1 — prueba tu TeamRepository en memoria directamente (sin mocks).
public class TeamRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ThenGetById_ReturnsSameTeam()
    {
        var repository = new TeamRepository();

        var created = await repository.CreateAsync(new Team { Name = "Eagles" });
        var found = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal("Eagles", found!.Name);
    }

    // TODO: PERSONA 1 — agregar el resto de casos (GetAll vacío/con datos, Update, Delete)
}
