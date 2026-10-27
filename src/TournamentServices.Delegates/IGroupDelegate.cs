using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface IGroupDelegate
{
    // Regresa todos los grupos de un torneo.
    Task<Result<IReadOnlyList<Group>>> GetByTournamentAsync(string tournamentId);
    // Busca un grupo por su id.
    Task<Result<Group>> GetByIdAsync(string tournamentId, string groupId);
    // Crea un grupo en el torneo.
    Task<Result<Group>> CreateAsync(string tournamentId, string name);
    // Cambia el nombre de un grupo.
    Task<Result<Group>> UpdateAsync(string tournamentId, string groupId, string name);
    // Borra un grupo.
    Task<Result<Unit>> DeleteAsync(string tournamentId, string groupId);
    // Agrega equipos a un grupo.
    Task<Result<Unit>> AssignTeamsAsync(string tournamentId, string groupId, IEnumerable<string> teamIds);
}
