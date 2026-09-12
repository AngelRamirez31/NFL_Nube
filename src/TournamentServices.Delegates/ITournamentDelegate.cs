using TournamentServices.Domain;

namespace TournamentServices.Delegates;

public interface ITournamentDelegate
{
    Task<IReadOnlyList<Tournament>> GetAllAsync();
    Task<Tournament?> GetByIdAsync(string id);
    Task<Tournament> CreateAsync(Tournament tournament);
    Task<Tournament?> UpdateAsync(string id, Tournament tournament);
    Task<Tournament?> PatchAsync(string id, string? name, int? numberOfGroups, int? maxTeamsPerGroup, string? type);
    Task<bool> DeleteAsync(string id);
}
