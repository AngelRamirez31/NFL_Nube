using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public interface IMatchRepository
{
    Task<IReadOnlyList<Match>> GetByTournamentAsync(string tournamentId);
    Task<Match?> GetByIdAsync(string tournamentId, string matchId);
    Task<Match> CreateAsync(Match match);
    Task<Match?> UpdateAsync(string matchId, Match match);
    Task<bool> DeleteAsync(string matchId);
}
