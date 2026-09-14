using Npgsql;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// ============================================================
// PERSONA 2 — Tournaments
// TODO: implementar contra Postgres siguiendo el patrón de TeamRepository.cs
// (Guid.TryParse al entrar -> null/false, id en la columna no en el document).
//
// SQL confirmado en el proyecto de referencia del profesor
// (PostgresConnectionProvider.hpp) — cópialo, no lo reinventes:
//
//   insert into TOURNAMENTS (document) values (@document::jsonb) returning id;
//   select id, document from TOURNAMENTS where id = @id;
//
// Completar (el C++ no los tenía implementados):
//   select id, document from TOURNAMENTS;
//   update TOURNAMENTS set document = @document::jsonb, last_update_date = CURRENT_TIMESTAMP
//     where id = @id returning id;
//   delete from TOURNAMENTS where id = @id;
//
// Recuerda: DELETE debe fallar con 23503 (FK) si el torneo tiene grupos —
// el cascade real (matches -> groups -> tournament) se resuelve en el
// Delegate/integración final, no aquí.
// ============================================================
public class TournamentRepository : ITournamentRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public TournamentRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public Task<IReadOnlyList<Tournament>> GetAllAsync()
    {
        // TODO: PERSONA 2
        throw new NotImplementedException();
    }

    public Task<Tournament?> GetByIdAsync(string id)
    {
        // TODO: PERSONA 2
        throw new NotImplementedException();
    }

    public Task<Tournament> CreateAsync(Tournament tournament)
    {
        // TODO: PERSONA 2
        throw new NotImplementedException();
    }

    public Task<Tournament?> UpdateAsync(string id, Tournament tournament)
    {
        // TODO: PERSONA 2 — usado tanto por PUT (todos los campos) como por el
        // read-modify-write de PATCH (el merge de campos se hace en el Delegate,
        // aquí solo se persiste el documento completo ya mergeado).
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string id)
    {
        // TODO: PERSONA 2
        throw new NotImplementedException();
    }
}
