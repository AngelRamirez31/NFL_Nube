using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface IMatchDelegate
{
    // Regresa todos los partidos de un torneo.
    Task<Result<IReadOnlyList<Match>>> GetByTournamentAsync(string tournamentId);
    // Busca un partido por su id.
    Task<Result<Match>> GetByIdAsync(string tournamentId, string matchId);
    // Crea un partido entre dos equipos.
    Task<Result<Match>> CreateAsync(string tournamentId, string? groupId, string homeTeamId, string visitorTeamId);
    // Guarda el marcador de un partido.
    Task<Result<Match>> UpdateScoreAsync(string tournamentId, string matchId, int homeScore, int visitorScore);
    // Borra un partido.
    Task<Result<Unit>> DeleteAsync(string tournamentId, string matchId);
}
