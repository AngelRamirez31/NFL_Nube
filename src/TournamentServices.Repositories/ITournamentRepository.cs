using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public interface ITournamentRepository
{
    // Regresa todos los torneos.
    Task<IReadOnlyList<Tournament>> GetAllAsync();
    // Busca un torneo por su id.
    Task<Tournament?> GetByIdAsync(string id);
    // Guarda un torneo nuevo.
    Task<Tournament> CreateAsync(Tournament tournament);
    // Actualiza un torneo.
    Task<Tournament?> UpdateAsync(string id, Tournament tournament);
    // Borra un torneo.
    Task<bool> DeleteAsync(string id);
}
