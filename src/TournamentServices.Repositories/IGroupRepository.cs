using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public interface IGroupRepository
{
    // Regresa los grupos de un torneo.
    Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId);
    // Busca un grupo por su id dentro del torneo.
    Task<Group?> GetByIdAsync(string tournamentId, string groupId);
    // Guarda un grupo nuevo.
    Task<Group> CreateAsync(Group group);
    // Actualiza un grupo.
    Task<Group?> UpdateAsync(string groupId, Group group);
    // Borra un grupo.
    Task<bool> DeleteAsync(string groupId);
    // Revisa si ya hay un grupo con ese nombre en el torneo.
    Task<bool> ExistsByNameInTournamentAsync(string tournamentId, string name);
    // Busca el grupo del torneo donde está un equipo.
    Task<Group?> FindByTournamentAndTeamAsync(string tournamentId, string teamId);
}
