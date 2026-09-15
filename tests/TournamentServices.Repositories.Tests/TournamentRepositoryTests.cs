using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Domain.Enums;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Repositories.Tests;

// Requieren la BD levantada (`podman compose up -d`).
// `dotnet test --filter "Category!=Integration"` las salta.
[Trait("Category", "Integration")]
public class TournamentRepositoryTests
{
    private static NpgsqlDataSource CreateDataSource() =>
        NpgsqlDataSource.Create("Host=localhost;Port=5432;Database=tournament_db;Username=tournament_svc;Password=password");

    private static Tournament NewTournament() => new()
    {
        Name = $"NFL-{Guid.NewGuid()}",
        Format = new TournamentFormat { NumberOfGroups = 8, MaxTeamsPerGroup = 4, Type = TournamentType.NFL }
    };

    [Fact]
    public async Task CreateAsync_ThenGetById_ReturnsSameTournament()
    {
        await using var dataSource = CreateDataSource();
        var repository = new TournamentRepository(dataSource);

        var created = await repository.CreateAsync(NewTournament());
        var found = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Name, found!.Name);
        Assert.Equal(8, found.Format.NumberOfGroups);
        Assert.Equal(TournamentType.NFL, found.Format.Type);

        await repository.DeleteAsync(created.Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsTheWholeDocument()
    {
        await using var dataSource = CreateDataSource();
        var repository = new TournamentRepository(dataSource);

        var created = await repository.CreateAsync(NewTournament());
        created.Name = $"Renombrado-{Guid.NewGuid()}";
        created.Format.MaxTeamsPerGroup = 6;
        await repository.UpdateAsync(created.Id, created);

        var found = await repository.GetByIdAsync(created.Id);

        Assert.Equal(created.Name, found!.Name);
        Assert.Equal(6, found.Format.MaxTeamsPerGroup);

        await repository.DeleteAsync(created.Id);
    }

    [Fact]
    public async Task InvalidUuidFormat_ReturnsSafeResultsInsteadOfThrowing()
    {
        await using var dataSource = CreateDataSource();
        var repository = new TournamentRepository(dataSource);

        Assert.Null(await repository.GetByIdAsync("no-es-un-uuid"));
        Assert.False(await repository.DeleteAsync("no-es-un-uuid"));
    }
}
