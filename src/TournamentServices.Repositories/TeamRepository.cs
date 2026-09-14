using Dapper;
using Npgsql;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public TeamRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<Team>> GetAllAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var rows = await connection.QueryAsync<TeamRow>("select id, document from TEAMS");
        return rows.Select(ToTeam).ToList();
    }

    public async Task<Team?> GetByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var teamId)) return null;

        await using var connection = await _dataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleOrDefaultAsync<TeamRow>(
            "select id, document from TEAMS where id = @teamId", new { teamId });

        return row is null ? null : ToTeam(row);
    }

    public async Task<Team> CreateAsync(Team team)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var document = DocumentSerializer.To(team);

        var newId = await connection.ExecuteScalarAsync<Guid>(
            "insert into TEAMS (document) values (@document::jsonb) returning id",
            new { document });

        team.Id = newId.ToString();
        return team;
    }

    public async Task<Team?> UpdateAsync(string id, Team team)
    {
        if (!Guid.TryParse(id, out var teamId)) return null;

        await using var connection = await _dataSource.OpenConnectionAsync();
        team.Id = id;
        var document = DocumentSerializer.To(team);

        var updatedId = await connection.ExecuteScalarAsync<Guid?>(
            """
            update TEAMS set document = @document::jsonb, last_update_date = CURRENT_TIMESTAMP
            where id = @teamId returning id
            """,
            new { teamId, document });

        return updatedId is null ? null : team;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var teamId)) return false;

        await using var connection = await _dataSource.OpenConnectionAsync();
        var affected = await connection.ExecuteAsync("delete from TEAMS where id = @teamId", new { teamId });
        return affected > 0;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var id = await connection.ExecuteScalarAsync<Guid?>(
            "select id from TEAMS where document->>'name' = @name", new { name });
        return id is not null;
    }

    private static Team ToTeam(TeamRow row)
    {
        var team = DocumentSerializer.From<Team>(row.Document);
        team.Id = row.Id.ToString();
        return team;
    }

    private record TeamRow(Guid Id, string Document);
}
