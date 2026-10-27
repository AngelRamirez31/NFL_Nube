using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface ITournamentDelegate
{
    // Regresa todos los torneos.
    Task<Result<IReadOnlyList<Tournament>>> GetAllAsync();
    // Busca un torneo con sus grupos y partidos.
    Task<Result<Tournament>> GetByIdAsync(string id);
    // Crea un torneo.
    Task<Result<Tournament>> CreateAsync(Tournament tournament);
    // Reemplaza todos los datos de un torneo.
    Task<Result<Tournament>> UpdateAsync(string id, Tournament tournament);
    // Cambia solo los campos que se manden.
    Task<Result<Tournament>> PatchAsync(string id, string? name, int? numberOfGroups, int? maxTeamsPerGroup, string? type);
    // Borra un torneo con sus grupos y partidos.
    Task<Result<Unit>> DeleteAsync(string id);
    // Genera los partidos de la siguiente fase.
    Task<Result<IReadOnlyList<Match>>> GenerateMatchesAsync(string id);
    // Regresa la tabla de posiciones de cada grupo.
    Task<Result<IReadOnlyList<GroupStandings>>> GetStandingsAsync(string id);
    // Regresa el campeón (ganador del Super Bowl).
    Task<Result<Team>> GetChampionAsync(string id);
}
