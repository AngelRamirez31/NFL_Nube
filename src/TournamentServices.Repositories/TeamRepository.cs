using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// ============================================================
// PERSONA 1 — Teams
// TODO: implementar esta clase con una lista/diccionario en memoria.
// No necesitas base de datos para este primer avance.
// ============================================================
public class TeamRepository : ITeamRepository
{
    private readonly List<Team> _teams = new();

    public Task<IReadOnlyList<Team>> GetAllAsync()
    {
        // TODO: PERSONA 1
        throw new NotImplementedException();
    }

    public Task<Team?> GetByIdAsync(string id)
    {
        // TODO: PERSONA 1
        throw new NotImplementedException();
    }

    public Task<Team> CreateAsync(Team team)
    {
        // TODO: PERSONA 1 — recuerda generar el Id (p.ej. Guid.NewGuid().ToString())
        throw new NotImplementedException();
    }

    public Task<Team?> UpdateAsync(string id, Team team)
    {
        // TODO: PERSONA 1
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string id)
    {
        // TODO: PERSONA 1
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByNameAsync(string name)
    {
        // TODO: PERSONA 1 — usado para validar nombre único al crear/actualizar
        throw new NotImplementedException();
    }
}
