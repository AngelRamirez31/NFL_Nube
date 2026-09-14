using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public interface IGroupRepository
{
    Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId);
    Task<Group?> GetByIdAsync(string tournamentId, string groupId);
    Task<Group> CreateAsync(Group group);
    Task<Group?> UpdateAsync(string groupId, Group group);
    Task<bool> DeleteAsync(string groupId);
    Task<bool> ExistsByNameInTournamentAsync(string tournamentId, string name);

    // Agregado por el Paso 0: ¿este equipo ya está en algún grupo de este torneo?
    // Es el select_group_in_tournament del profesor (containment sobre el JSONB).
    Task<Group?> FindByTournamentAndTeamAsync(string tournamentId, string teamId);
}
