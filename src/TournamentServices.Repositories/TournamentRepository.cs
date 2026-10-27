using Dapper;
using Npgsql;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public class TournamentRepository : ITournamentRepository
{
    private readonly NpgsqlDataSource _dataSource;

    // Recibe la conexión a la base de datos.
    public TournamentRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Regresa todos los torneos.
    public async Task<IReadOnlyList<Tournament>> GetAllAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        var rows = await connection.QueryAsync<TournamentRow>("select id, document from TOURNAMENTS");

        return rows.Select(ToTournament).ToList();
    }

    // Busca un torneo por su id.
    public async Task<Tournament?> GetByIdAsync(string id)
    {
        // Si el id no es un UUID válido, se trata como no encontrado.
        if (!Guid.TryParse(id, out var tournamentId)) return null;

        await using var connection = await _dataSource.OpenConnectionAsync();

        var row = await connection.QuerySingleOrDefaultAsync<TournamentRow>(
            "select id, document from TOURNAMENTS where id = @tournamentId",
            new { tournamentId });

        return row is null ? null : ToTournament(row);
    }

    // Guarda un torneo nuevo y le pone el id que genera la base.
    public async Task<Tournament> CreateAsync(Tournament tournament)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var document = DocumentSerializer.To(tournament);

        var newId = await connection.ExecuteScalarAsync<Guid>(
            "insert into TOURNAMENTS (document) values (@document::jsonb) returning id",
            new { document });

        tournament.Id = newId.ToString();
        return tournament;
    }

    // Actualiza los datos de un torneo.
    public async Task<Tournament?> UpdateAsync(string id, Tournament tournament)
    {
        if (!Guid.TryParse(id, out var tournamentId)) return null;

        await using var connection = await _dataSource.OpenConnectionAsync();
        tournament.Id = id;
        var document = DocumentSerializer.To(tournament);

        var updatedId = await connection.ExecuteScalarAsync<Guid?>(
            """
            update TOURNAMENTS set document = @document::jsonb, last_update_date = CURRENT_TIMESTAMP
            where id = @tournamentId returning id
            """,
            new { tournamentId, document });

        return updatedId is null ? null : tournament;
    }

    // Borra un torneo por su id.
    public async Task<bool> DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var tournamentId)) return false;

        await using var connection = await _dataSource.OpenConnectionAsync();

        var affected = await connection.ExecuteAsync(
            "delete from TOURNAMENTS where id = @tournamentId",
            new { tournamentId });

        return affected > 0;
    }

    // Convierte una fila de la base a un Tournament.
    private static Tournament ToTournament(TournamentRow row)
    {
        var tournament = DocumentSerializer.From<Tournament>(row.Document);
        tournament.Id = row.Id.ToString();
        return tournament;
    }

    // Fila tal como viene de la tabla TOURNAMENTS.
    private record TournamentRow(Guid Id, string Document);
}
