using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface ITournamentDelegate
{
    Task<Result<IReadOnlyList<Tournament>>> GetAllAsync();
    Task<Result<Tournament>> GetByIdAsync(string id);
    Task<Result<Tournament>> CreateAsync(Tournament tournament);
    Task<Result<Tournament>> UpdateAsync(string id, Tournament tournament);
    Task<Result<Tournament>> PatchAsync(string id, string? name, int? numberOfGroups, int? maxTeamsPerGroup, string? type);
    Task<Result<Unit>> DeleteAsync(string id);
}
