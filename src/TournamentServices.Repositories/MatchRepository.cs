using Npgsql;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// ============================================================
// PERSONA 4 — Matches
// TODO: implementar contra Postgres siguiendo el patrón de TeamRepository.cs.
// Mientras Teams/Groups reales no estén listos, usa los fakes de
// tests/TournamentServices.Delegates.Tests/Fakes/ SOLO en tus propios tests.
//
// OJO: la tabla MATCHES NO tiene columna tournament_id — se filtra por el
// document (por eso database/002_matches_index.sql agrega un índice sobre
// document->>'tournamentId').
//
//   insert into MATCHES (document) values (@document::jsonb) returning id;
//   select id, document from MATCHES where id = @id;
//   select id, document from MATCHES where document->>'tournamentId' = @tournamentId;
//   update MATCHES set document = jsonb_set(document, '{score}', @score::jsonb),
//                      last_update_date = CURRENT_TIMESTAMP
//   where id = @id returning id;
//   delete from MATCHES where id = @id;
// ============================================================
public class MatchRepository : IMatchRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public MatchRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public Task<IReadOnlyList<Match>> GetByTournamentAsync(string tournamentId)
    {
        // TODO: PERSONA 4
        throw new NotImplementedException();
    }

    public Task<Match?> GetByIdAsync(string tournamentId, string matchId)
    {
        // TODO: PERSONA 4
        throw new NotImplementedException();
    }

    public Task<Match> CreateAsync(Match match)
    {
        // TODO: PERSONA 4
        throw new NotImplementedException();
    }

    public Task<Match?> UpdateAsync(string matchId, Match match)
    {
        // TODO: PERSONA 4 — usado por PATCH .../score (jsonb_set solo sobre "score")
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string matchId)
    {
        // TODO: PERSONA 4
        throw new NotImplementedException();
    }
}
