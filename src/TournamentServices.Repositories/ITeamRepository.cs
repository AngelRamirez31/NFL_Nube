using TournamentServices.Domain;

namespace TournamentServices.Repositories;

public interface ITeamRepository
{
    // Regresa todos los equipos.
    Task<IReadOnlyList<Team>> GetAllAsync();
    // Busca un equipo por su id.
    Task<Team?> GetByIdAsync(string id);
    // Guarda un equipo nuevo.
    Task<Team> CreateAsync(Team team);
    // Actualiza un equipo.
    Task<Team?> UpdateAsync(string id, Team team);
    // Borra un equipo.
    Task<bool> DeleteAsync(string id);
    // Revisa si ya existe un equipo con ese nombre.
    Task<bool> ExistsByNameAsync(string name);
}
