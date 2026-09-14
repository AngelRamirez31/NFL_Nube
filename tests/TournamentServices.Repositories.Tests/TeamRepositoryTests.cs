using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Repositories.Tests;

// PERSONA 1 — pruebas de integración REALES contra Postgres.
// Requieren la BD levantada (ver README: `podman compose up -d`).
// Corren aparte de las rápidas: `dotnet test --filter "Category!=Integration"`
// las salta si no tienes Podman encendido.
[Trait("Category", "Integration")]
public class TeamRepositoryTests
{
    private static NpgsqlDataSource CreateDataSource() =>
        NpgsqlDataSource.Create("Host=localhost;Port=5432;Database=tournament_db;Username=tournament_svc;Password=password");

    [Fact]
    public async Task CreateAsync_ThenGetById_ReturnsSameTeam()
    {
        await using var dataSource = CreateDataSource();
        var repository = new TeamRepository(dataSource);

        // Nombre único por corrida: los índices únicos castigan cualquier
        // dato reutilizado entre ejecuciones de la prueba.
        var uniqueName = $"Eagles-{Guid.NewGuid()}";
        var created = await repository.CreateAsync(new Team { Name = uniqueName });
        var found = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(uniqueName, found!.Name);

        await repository.DeleteAsync(created.Id); // limpieza
    }

    [Fact]
    public async Task GetByIdAsync_WithNonUuidFormat_ReturnsNullInsteadOfThrowing()
    {
        await using var dataSource = CreateDataSource();
        var repository = new TeamRepository(dataSource);

        var found = await repository.GetByIdAsync("no-es-un-uuid");

        Assert.Null(found); // y NO lanza PostgresException 22P02
    }

    // TODO: PERSONA 1 — agregar el resto de casos (GetAll, Update, Delete,
    // ExistsByNameAsync) y replicar este mismo patrón para tu TournamentRepositoryTests.
}
