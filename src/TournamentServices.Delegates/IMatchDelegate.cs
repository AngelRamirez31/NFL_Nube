using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface IMatchDelegate
{
    Task<Result<IReadOnlyList<Match>>> GetByTournamentAsync(string tournamentId);
    Task<Result<Match>> GetByIdAsync(string tournamentId, string matchId);
    Task<Result<Match>> CreateAsync(string tournamentId, string? groupId, string homeTeamId, string visitorTeamId);
    Task<Result<Match>> UpdateScoreAsync(string tournamentId, string matchId, int homeScore, int visitorScore);
    Task<Result<Unit>> DeleteAsync(string tournamentId, string matchId);
}
