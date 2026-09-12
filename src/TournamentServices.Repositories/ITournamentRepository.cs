using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public interface ITournamentRepository
{
    Task<IReadOnlyList<Tournament>> GetAllAsync();
    Task<Tournament?> GetByIdAsync(string id);
    Task<Tournament> CreateAsync(Tournament tournament);
    Task<Tournament?> UpdateAsync(string id, Tournament tournament);
    Task<bool> DeleteAsync(string id);
}
