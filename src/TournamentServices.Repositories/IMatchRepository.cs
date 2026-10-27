using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public interface IMatchRepository
{
    // Regresa los partidos de un torneo.
    Task<IReadOnlyList<Match>> GetByTournamentAsync(string tournamentId);
    // Busca un partido por su id dentro del torneo.
    Task<Match?> GetByIdAsync(string tournamentId, string matchId);
    // Guarda un partido nuevo.
    Task<Match> CreateAsync(Match match);
    // Actualiza el marcador de un partido.
    Task<Match?> UpdateAsync(string matchId, Match match);
    // Borra un partido.
    Task<bool> DeleteAsync(string matchId);
}
