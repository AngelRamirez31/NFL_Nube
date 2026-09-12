using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// ============================================================
// PERSONA 4 — Matches
// TODO: implementar esta clase con una lista/diccionario en memoria.
// Mientras Persona 1 y 3 no terminen, usa los fakes de Teams/Groups
// (ver carpeta Fakes/) para poder probar la creación de partidos.
// ============================================================
public class MatchRepository : IMatchRepository
{
    private readonly List<Match> _matches = new();

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
        // TODO: PERSONA 4 — genera el Id
        throw new NotImplementedException();
    }

    public Task<Match?> UpdateAsync(string matchId, Match match)
    {
        // TODO: PERSONA 4 — usado por PATCH .../score
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string matchId)
    {
        // TODO: PERSONA 4
        throw new NotImplementedException();
    }
}
