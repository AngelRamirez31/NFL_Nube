using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface ITeamDelegate
{
    Task<Result<IReadOnlyList<Team>>> GetAllAsync();
    Task<Result<Team>> GetByIdAsync(string id);
    Task<Result<Team>> CreateAsync(Team team);
    Task<Result<Team>> UpdateAsync(string id, Team team);
    Task<Result<Unit>> DeleteAsync(string id);
}
