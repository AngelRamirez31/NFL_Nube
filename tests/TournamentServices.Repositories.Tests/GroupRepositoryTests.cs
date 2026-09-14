using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Repositories;
using Xunit;

namespace TournamentServices.Repositories.Tests;

[Trait("Category", "Integration")]
public class GroupRepositoryTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=tournament_db;Username=tournament_svc;Password=password";

    private static NpgsqlDataSource CreateDataSource() =>
        NpgsqlDataSource.Create(ConnectionString);

    [Fact]
    public async Task CreateAsync_ThenGetById_ReturnsSameGroup()
    {
        await using var dataSource = CreateDataSource();
        var repository = new GroupRepository(dataSource);

        var tournamentId = await CreateTournamentAsync(dataSource);
        string? groupId = null;

        try
        {
            var group = new Group
            {
                Name = $"Group-{Guid.NewGuid()}",
                TournamentId = tournamentId,
                Teams = new List<Team>()
            };

            var created = await repository.CreateAsync(group);
            groupId = created.Id;

            var found = await repository.GetByIdAsync(
                tournamentId,
                created.Id);

            Assert.NotNull(found);
            Assert.Equal(created.Id, found!.Id);
            Assert.Equal(group.Name, found.Name);
            Assert.Equal(tournamentId, found.TournamentId);
            Assert.Empty(found.Teams);
        }
        finally
        {
            if (groupId is not null)
                await repository.DeleteAsync(groupId);

            await DeleteTournamentAsync(
                dataSource,
                tournamentId);
        }
    }

    [Fact]
    public async Task GetByTournamentAsync_ReturnsOnlyGroupsFromTournament()
    {
        await using var dataSource = CreateDataSource();
        var repository = new GroupRepository(dataSource);

        var tournamentId = await CreateTournamentAsync(dataSource);
        var otherTournamentId = await CreateTournamentAsync(dataSource);

        string? groupId1 = null;
        string? groupId2 = null;
        string? otherGroupId = null;

        try
        {
            var group1 = await repository.CreateAsync(new Group
            {
                Name = $"Group-A-{Guid.NewGuid()}",
                TournamentId = tournamentId
            });

            var group2 = await repository.CreateAsync(new Group
            {
                Name = $"Group-B-{Guid.NewGuid()}",
                TournamentId = tournamentId
            });

            var otherGroup = await repository.CreateAsync(new Group
            {
                Name = $"Group-C-{Guid.NewGuid()}",
                TournamentId = otherTournamentId
            });

            groupId1 = group1.Id;
            groupId2 = group2.Id;
            otherGroupId = otherGroup.Id;

            var groups = await repository.GetByTournamentAsync(
                tournamentId);

            Assert.Contains(groups, g => g.Id == group1.Id);
            Assert.Contains(groups, g => g.Id == group2.Id);
            Assert.DoesNotContain(
                groups,
                g => g.Id == otherGroup.Id);

            Assert.All(
                groups,
                g => Assert.Equal(
                    tournamentId,
                    g.TournamentId));
        }
        finally
        {
            if (groupId1 is not null)
                await repository.DeleteAsync(groupId1);

            if (groupId2 is not null)
                await repository.DeleteAsync(groupId2);

            if (otherGroupId is not null)
                await repository.DeleteAsync(otherGroupId);

            await DeleteTournamentAsync(
                dataSource,
                tournamentId);

            await DeleteTournamentAsync(
                dataSource,
                otherTournamentId);
        }
    }

    [Fact]
    public async Task UpdateAsync_PersistsNameAndTeams()
    {
        await using var dataSource = CreateDataSource();
        var repository = new GroupRepository(dataSource);

        var tournamentId = await CreateTournamentAsync(dataSource);
        string? groupId = null;

        try
        {
            var group = await repository.CreateAsync(new Group
            {
                Name = $"Original-{Guid.NewGuid()}",
                TournamentId = tournamentId
            });

            groupId = group.Id;

            var team = new Team
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Eagles"
            };

            group.Name = $"Updated-{Guid.NewGuid()}";
            group.Teams.Add(team);

            var updated = await repository.UpdateAsync(
                group.Id,
                group);

            Assert.NotNull(updated);

            var found = await repository.GetByIdAsync(
                tournamentId,
                group.Id);

            Assert.NotNull(found);
            Assert.Equal(group.Name, found!.Name);
            Assert.Single(found.Teams);
            Assert.Equal(team.Id, found.Teams[0].Id);
            Assert.Equal(team.Name, found.Teams[0].Name);
        }
        finally
        {
            if (groupId is not null)
                await repository.DeleteAsync(groupId);

            await DeleteTournamentAsync(
                dataSource,
                tournamentId);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesGroup()
    {
        await using var dataSource = CreateDataSource();
        var repository = new GroupRepository(dataSource);

        var tournamentId = await CreateTournamentAsync(dataSource);

        try
        {
            var group = await repository.CreateAsync(new Group
            {
                Name = $"Delete-{Guid.NewGuid()}",
                TournamentId = tournamentId
            });

            var deleted = await repository.DeleteAsync(group.Id);

            Assert.True(deleted);

            var found = await repository.GetByIdAsync(
                tournamentId,
                group.Id);

            Assert.Null(found);

            var deletedAgain =
                await repository.DeleteAsync(group.Id);

            Assert.False(deletedAgain);
        }
        finally
        {
            await DeleteTournamentAsync(
                dataSource,
                tournamentId);
        }
    }

    [Fact]
    public async Task ExistsByNameInTournamentAsync_FindsOnlyMatchingTournament()
    {
        await using var dataSource = CreateDataSource();
        var repository = new GroupRepository(dataSource);

        var tournamentId = await CreateTournamentAsync(dataSource);
        var otherTournamentId = await CreateTournamentAsync(dataSource);

        string? groupId = null;

        try
        {
            var name = $"Unique-{Guid.NewGuid()}";

            var group = await repository.CreateAsync(new Group
            {
                Name = name,
                TournamentId = tournamentId
            });

            groupId = group.Id;

            var exists =
                await repository.ExistsByNameInTournamentAsync(
                    tournamentId,
                    name);

            var existsInOtherTournament =
                await repository.ExistsByNameInTournamentAsync(
                    otherTournamentId,
                    name);

            Assert.True(exists);
            Assert.False(existsInOtherTournament);
        }
        finally
        {
            if (groupId is not null)
                await repository.DeleteAsync(groupId);

            await DeleteTournamentAsync(
                dataSource,
                tournamentId);

            await DeleteTournamentAsync(
                dataSource,
                otherTournamentId);
        }
    }

    [Fact]
    public async Task FindByTournamentAndTeamAsync_ReturnsContainingGroup()
    {
        await using var dataSource = CreateDataSource();
        var repository = new GroupRepository(dataSource);

        var tournamentId = await CreateTournamentAsync(dataSource);
        string? groupId = null;

        try
        {
            var team = new Team
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cowboys"
            };

            var group = await repository.CreateAsync(new Group
            {
                Name = $"TeamLookup-{Guid.NewGuid()}",
                TournamentId = tournamentId,
                Teams = new List<Team> { team }
            });

            groupId = group.Id;

            var found =
                await repository.FindByTournamentAndTeamAsync(
                    tournamentId,
                    team.Id);

            Assert.NotNull(found);
            Assert.Equal(group.Id, found!.Id);
            Assert.Contains(
                found.Teams,
                t => t.Id == team.Id);
        }
        finally
        {
            if (groupId is not null)
                await repository.DeleteAsync(groupId);

            await DeleteTournamentAsync(
                dataSource,
                tournamentId);
        }
    }

    [Fact]
    public async Task InvalidUuidFormats_ReturnSafeResultsInsteadOfThrowing()
    {
        await using var dataSource = CreateDataSource();
        var repository = new GroupRepository(dataSource);

        var groups =
            await repository.GetByTournamentAsync(
                "not-a-uuid");

        var group =
            await repository.GetByIdAsync(
                "not-a-uuid",
                "also-not-a-uuid");

        var deleted =
            await repository.DeleteAsync(
                "not-a-uuid");

        var exists =
            await repository.ExistsByNameInTournamentAsync(
                "not-a-uuid",
                "Group");

        var groupByTeam =
            await repository.FindByTournamentAndTeamAsync(
                "not-a-uuid",
                "not-a-uuid");

        Assert.Empty(groups);
        Assert.Null(group);
        Assert.False(deleted);
        Assert.False(exists);
        Assert.Null(groupByTeam);
    }

    private static async Task<string> CreateTournamentAsync(
        NpgsqlDataSource dataSource)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            insert into TOURNAMENTS (document)
            values (cast(@document as jsonb))
            returning id
            """;

        command.Parameters.AddWithValue(
            "document",
            $$"""
            {
              "name": "Integration-{{Guid.NewGuid()}}"
            }
            """);

        var result = await command.ExecuteScalarAsync();

        return ((Guid)result!).ToString();
    }

    private static async Task DeleteTournamentAsync(
        NpgsqlDataSource dataSource,
        string tournamentId)
    {
        if (!Guid.TryParse(
                tournamentId,
                out var parsedTournamentId))
        {
            return;
        }

        await using var connection =
            await dataSource.OpenConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            delete from TOURNAMENTS
            where id = @id
            """;

        command.Parameters.AddWithValue(
            "id",
            parsedTournamentId);

        await command.ExecuteNonQueryAsync();
    }
}