using TournamentServices.Domain;
using TournamentServices.Domain.Common;

namespace TournamentServices.Delegates;

public interface ITeamDelegate
{
    // Regresa todos los equipos.
    Task<Result<IReadOnlyList<Team>>> GetAllAsync();
    // Busca un equipo por su id.
    Task<Result<Team>> GetByIdAsync(string id);
    // Crea un equipo.
    Task<Result<Team>> CreateAsync(Team team);
    // Actualiza un equipo.
    Task<Result<Team>> UpdateAsync(string id, Team team);
    // Borra un equipo.
    Task<Result<Unit>> DeleteAsync(string id);
}
