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
    Task<Group?> FindByTournamentAndTeamAsync(string tournamentId, string teamId);
}
