using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface IGroupDelegate
{
    Task<Result<IReadOnlyList<Group>>> GetByTournamentAsync(string tournamentId);
    Task<Result<Group>> GetByIdAsync(string tournamentId, string groupId);
    Task<Result<Group>> CreateAsync(string tournamentId, string name);
    Task<Result<Group>> UpdateAsync(string tournamentId, string groupId, string name);
    Task<Result<Unit>> DeleteAsync(string tournamentId, string groupId);
    Task<Result<Unit>> AssignTeamsAsync(string tournamentId, string groupId, IEnumerable<string> teamIds);
}
