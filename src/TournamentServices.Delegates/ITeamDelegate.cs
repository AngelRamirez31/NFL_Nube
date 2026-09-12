using TournamentServices.Domain;

namespace TournamentServices.Delegates;

public interface ITeamDelegate
{
    Task<IReadOnlyList<Team>> GetAllAsync();
    Task<Team?> GetByIdAsync(string id);
    Task<Team> CreateAsync(Team team);
    Task<Team?> UpdateAsync(string id, Team team);
    Task<bool> DeleteAsync(string id);
}
