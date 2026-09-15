using Dapper;
using Npgsql;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// MATCHES no tiene columna tournament_id: se filtra por document->>'tournamentId'
// (índice en database/002_matches_index.sql). Guid.TryParse antes de cada query
// evita que un id no-UUID tire 22P02 en vez de devolver null/false.
public class MatchRepository : IMatchRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public MatchRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<Match>> GetByTournamentAsync(string tournamentId)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var rows = await connection.QueryAsync<MatchRow>(
            "select id, document from MATCHES where document->>'tournamentId' = @tournamentId",
            new { tournamentId });

        return rows.Select(ToMatch).ToList();
    }

    public async Task<Match?> GetByIdAsync(string tournamentId, string matchId)
    {
        if (!Guid.TryParse(matchId, out var id)) return null;

        await using var connection = await _dataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleOrDefaultAsync<MatchRow>(
            "select id, document from MATCHES where id = @id and document->>'tournamentId' = @tournamentId",
            new { id, tournamentId });

        return row is null ? null : ToMatch(row);
    }

    public async Task<Match> CreateAsync(Match match)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var document = DocumentSerializer.To(match);

        var newId = await connection.ExecuteScalarAsync<Guid>(
            "insert into MATCHES (document) values (@document::jsonb) returning id",
            new { document });

        match.Id = newId.ToString();
        return match;
    }

    public async Task<Match?> UpdateAsync(string matchId, Match match)
    {
        if (!Guid.TryParse(matchId, out var id)) return null;

        await using var connection = await _dataSource.OpenConnectionAsync();
        var score = DocumentSerializer.To(match.Score);

        var updatedId = await connection.ExecuteScalarAsync<Guid?>(
            """
            update MATCHES set document = jsonb_set(document, '{score}', @score::jsonb),
                               last_update_date = CURRENT_TIMESTAMP
            where id = @id returning id
            """,
            new { id, score });

        if (updatedId is null) return null;

        match.Id = id.ToString();
        return match;
    }

    public async Task<bool> DeleteAsync(string matchId)
    {
        if (!Guid.TryParse(matchId, out var id)) return false;

        await using var connection = await _dataSource.OpenConnectionAsync();
        var affected = await connection.ExecuteAsync("delete from MATCHES where id = @id", new { id });
        return affected > 0;
    }

    private static Match ToMatch(MatchRow row)
    {
        var match = DocumentSerializer.From<Match>(row.Document);
        match.Id = row.Id.ToString();
        return match;
    }

    private record MatchRow(Guid Id, string Document);
}
