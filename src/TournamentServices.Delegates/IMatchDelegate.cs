using TournamentServices.Domain;

namespace TournamentServices.Delegates;

public interface IMatchDelegate
{
    Task<IReadOnlyList<Match>> GetByTournamentAsync(string tournamentId);
    Task<Match?> GetByIdAsync(string tournamentId, string matchId);

    /// <returns>null = 404/422 según corresponda (equipo no existe, mismo equipo, no está en el torneo)</returns>
    Task<Match?> CreateAsync(string tournamentId, string? groupId, string homeTeamId, string visitorTeamId);
    Task<Match?> UpdateScoreAsync(string tournamentId, string matchId, int homeScore, int visitorScore);
    Task<bool> DeleteAsync(string tournamentId, string matchId);
}
