using Dapper;
using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Domain.Enums;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Repositories.Tests;

// PERSONA 4 — pruebas de integración REALES contra Postgres.
// Requieren la BD levantada (ver README: `podman compose up -d`).
// `dotnet test --filter "Category!=Integration"` las salta si no tienes
// Podman encendido.
[Trait("Category", "Integration")]
public class MatchRepositoryTests
{
    private static NpgsqlDataSource CreateDataSource() =>
        NpgsqlDataSource.Create("Host=localhost;Port=5432;Database=tournament_db;Username=tournament_svc;Password=password");

    // Id de torneo único por corrida: MATCHES no tiene columna tournament_id
    // propia, así que aislar por tournamentId es lo que evita que una prueba
    // vea los partidos de otra.
    private static string NewTournamentId() => Guid.NewGuid().ToString();

    [Fact]
    public async Task CreateAsync_AssignsIdGeneratedByPostgres()
    {
        await using var dataSource = CreateDataSource();
        var repository = new MatchRepository(dataSource);

        var created = await repository.CreateAsync(new Match
        {
            TournamentId = NewTournamentId(),
            HomeTeamId = "team-1",
            VisitorTeamId = "team-2"
        });

        Assert.True(Guid.TryParse(created.Id, out _));

        await repository.DeleteAsync(created.Id); // limpieza
    }

    [Fact]
    public async Task GetByIdAsync_RoundTripsTheDocument()
    {
        await using var dataSource = CreateDataSource();
        var repository = new MatchRepository(dataSource);
        var tournamentId = NewTournamentId();

        var created = await repository.CreateAsync(new Match
        {
            TournamentId = tournamentId,
            GroupId = "group-x",
            HomeTeamId = "team-home",
            VisitorTeamId = "team-visitor"
        });

        var found = await repository.GetByIdAsync(tournamentId, created.Id);

        Assert.NotNull(found);
        Assert.Equal("team-home", found!.HomeTeamId);
        Assert.Equal("team-visitor", found.VisitorTeamId);
        Assert.Equal("group-x", found.GroupId);
        Assert.Null(found.Score);
        Assert.False(found.IsCompleted);

        await repository.DeleteAsync(created.Id); // limpieza
    }

    [Fact]
    public async Task UpdateAsync_PersistsScoreAndComputedWinnerSurvivesTheRoundTrip()
    {
        await using var dataSource = CreateDataSource();
        var repository = new MatchRepository(dataSource);
        var tournamentId = NewTournamentId();

        var created = await repository.CreateAsync(new Match
        {
            TournamentId = tournamentId,
            HomeTeamId = "team-1",
            VisitorTeamId = "team-2"
        });

        created.Score = new Score { HomeTeamScore = 27, VisitorTeamScore = 24 };
        await repository.UpdateAsync(created.Id, created);

        var found = await repository.GetByIdAsync(tournamentId, created.Id);

        Assert.Equal(27, found!.Score!.HomeTeamScore);
        Assert.Equal(Winner.HOME, found.Winner);
        Assert.True(found.IsCompleted);

        await repository.DeleteAsync(created.Id); // limpieza
    }

    [Fact]
    public async Task GetByTournamentAsync_OnlyReturnsMatchesOfThatTournament()
    {
        await using var dataSource = CreateDataSource();
        var repository = new MatchRepository(dataSource);
        var mine = NewTournamentId();
        var other = NewTournamentId();

        var m1 = await repository.CreateAsync(new Match { TournamentId = mine, HomeTeamId = "team-1", VisitorTeamId = "team-2" });
        var m2 = await repository.CreateAsync(new Match { TournamentId = mine, HomeTeamId = "team-1", VisitorTeamId = "team-2" });
        var m3 = await repository.CreateAsync(new Match { TournamentId = other, HomeTeamId = "team-1", VisitorTeamId = "team-2" });

        var matches = await repository.GetByTournamentAsync(mine);

        Assert.Equal(2, matches.Count);
        Assert.All(matches, m => Assert.Equal(mine, m.TournamentId));

        await repository.DeleteAsync(m1.Id);
        await repository.DeleteAsync(m2.Id);
        await repository.DeleteAsync(m3.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonUuidFormat_ReturnsNullInsteadOfThrowing()
    {
        await using var dataSource = CreateDataSource();
        var repository = new MatchRepository(dataSource);

        var found = await repository.GetByIdAsync(NewTournamentId(), "no-es-un-uuid");

        Assert.Null(found); // y NO lanza PostgresException 22P02
    }

    [Fact]
    public async Task DeleteAsync_WithNonUuidFormat_ReturnsFalseInsteadOfThrowing()
    {
        await using var dataSource = CreateDataSource();
        var repository = new MatchRepository(dataSource);

        Assert.False(await repository.DeleteAsync("no-es-un-uuid"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheMatch()
    {
        await using var dataSource = CreateDataSource();
        var repository = new MatchRepository(dataSource);
        var tournamentId = NewTournamentId();

        var created = await repository.CreateAsync(new Match
        {
            TournamentId = tournamentId,
            HomeTeamId = "team-1",
            VisitorTeamId = "team-2"
        });

        Assert.True(await repository.DeleteAsync(created.Id));
        Assert.Null(await repository.GetByIdAsync(tournamentId, created.Id));
    }
}
