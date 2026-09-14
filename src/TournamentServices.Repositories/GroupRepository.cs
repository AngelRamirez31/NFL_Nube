using Dapper;
using Npgsql;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public GroupRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var parsedTournamentId))
            return Array.Empty<Group>();

        await using var connection = await _dataSource.OpenConnectionAsync();

        var rows = await connection.QueryAsync<GroupRow>(
            """
            select id, tournament_id as TournamentId, document
            from GROUPS
            where tournament_id = @parsedTournamentId
            """,
            new { parsedTournamentId });

        return rows.Select(ToGroup).ToList();
    }

    public async Task<Group?> GetByIdAsync(string tournamentId, string groupId)
    {
        if (!Guid.TryParse(tournamentId, out var parsedTournamentId))
            return null;

        if (!Guid.TryParse(groupId, out var parsedGroupId))
            return null;

        await using var connection = await _dataSource.OpenConnectionAsync();

        var row = await connection.QuerySingleOrDefaultAsync<GroupRow>(
            """
            select id, tournament_id as TournamentId, document
            from GROUPS
            where tournament_id = @parsedTournamentId
              and id = @parsedGroupId
            """,
            new
            {
                parsedTournamentId,
                parsedGroupId
            });

        return row is null ? null : ToGroup(row);
    }

    public async Task<Group> CreateAsync(Group group)
    {
        if (!Guid.TryParse(group.TournamentId, out var tournamentId))
            throw new ArgumentException(
                "TournamentId must be a valid GUID.",
                nameof(group));

        group.TournamentId = tournamentId.ToString();
        group.Teams ??= new List<Team>();

        await using var connection = await _dataSource.OpenConnectionAsync();

        var document = ToDocument(group);

        var newId = await connection.ExecuteScalarAsync<Guid>(
            """
            insert into GROUPS (tournament_id, document)
            values (@tournamentId, @document::jsonb)
            returning id
            """,
            new
            {
                tournamentId,
                document
            });

        group.Id = newId.ToString();

        return group;
    }

    public async Task<Group?> UpdateAsync(string groupId, Group group)
    {
        if (!Guid.TryParse(groupId, out var parsedGroupId))
            return null;

        if (!Guid.TryParse(group.TournamentId, out var parsedTournamentId))
            return null;

        group.Id = parsedGroupId.ToString();
        group.TournamentId = parsedTournamentId.ToString();
        group.Teams ??= new List<Team>();

        await using var connection = await _dataSource.OpenConnectionAsync();

        var document = ToDocument(group);

        var updatedId = await connection.ExecuteScalarAsync<Guid?>(
            """
            update GROUPS
            set document = @document::jsonb,
                last_update_date = CURRENT_TIMESTAMP
            where tournament_id = @parsedTournamentId
              and id = @parsedGroupId
            returning id
            """,
            new
            {
                parsedTournamentId,
                parsedGroupId,
                document
            });

        return updatedId is null ? null : group;
    }

    public async Task<bool> DeleteAsync(string groupId)
    {
        if (!Guid.TryParse(groupId, out var parsedGroupId))
            return false;

        await using var connection = await _dataSource.OpenConnectionAsync();

        var affected = await connection.ExecuteAsync(
            """
            delete from GROUPS
            where id = @parsedGroupId
            """,
            new { parsedGroupId });

        return affected > 0;
    }

    public async Task<bool> ExistsByNameInTournamentAsync(
        string tournamentId,
        string name)
    {
        if (!Guid.TryParse(tournamentId, out var parsedTournamentId))
            return false;

        await using var connection = await _dataSource.OpenConnectionAsync();

        var result = await connection.ExecuteScalarAsync<int?>(
            """
            select 1
            from GROUPS
            where tournament_id = @parsedTournamentId
              and document->>'name' = @name
            limit 1
            """,
            new
            {
                parsedTournamentId,
                name
            });

        return result is not null;
    }

    public async Task<Group?> FindByTournamentAndTeamAsync(
        string tournamentId,
        string teamId)
    {
        if (!Guid.TryParse(tournamentId, out var parsedTournamentId))
            return null;

        if (!Guid.TryParse(teamId, out var parsedTeamId))
            return null;

        var normalizedTeamId = parsedTeamId.ToString();

        await using var connection = await _dataSource.OpenConnectionAsync();

        var row = await connection.QuerySingleOrDefaultAsync<GroupRow>(
            """
            select id, tournament_id as TournamentId, document
            from GROUPS
            where tournament_id = @parsedTournamentId
              and document @> jsonb_build_object(
                    'teams',
                    jsonb_build_array(
                        jsonb_build_object('id', @normalizedTeamId::text)
                    )
                  )
            limit 1
            """,
            new
            {
                parsedTournamentId,
                normalizedTeamId
            });

        return row is null ? null : ToGroup(row);
    }

    private static Group ToGroup(GroupRow row)
    {
        var group = DocumentSerializer.From<Group>(row.Document);

        group.Id = row.Id.ToString();
        group.TournamentId = row.TournamentId.ToString();
        group.Teams ??= new List<Team>();

        return group;
    }

    private static string ToDocument(Group group)
    {
        return DocumentSerializer.To(new
        {
            group.Name,
            group.TournamentId,
            Teams = group.Teams ?? new List<Team>()
        });
    }

    private sealed record GroupRow(
        Guid Id,
        Guid TournamentId,
        string Document);
}