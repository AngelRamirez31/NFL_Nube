using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// ============================================================
// PERSONA 3 — Groups
// TODO: implementar esta clase con una lista/diccionario en memoria.
// Mientras Persona 1 y 2 no terminen sus repos reales, usa
// FakeTeamRepository / FakeTournamentRepository (ver carpeta Fakes/)
// para poder probar la asignación de equipos sin bloquearte.
// ============================================================
public class GroupRepository : IGroupRepository
{
    private readonly List<Group> _groups = new();

    public Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }

    public Task<Group?> GetByIdAsync(string tournamentId, string groupId)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }

    public Task<Group> CreateAsync(Group group)
    {
        // TODO: PERSONA 3 — genera el Id
        throw new NotImplementedException();
    }

    public Task<Group?> UpdateAsync(string groupId, Group group)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string groupId)
    {
        // TODO: PERSONA 3 — recuerda "desasignar" los equipos del grupo, no borrarlos
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByNameInTournamentAsync(string tournamentId, string name)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }
}
