using TournamentServices.Domain;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 3 — Groups
// TODO: orquesta IGroupRepository + ITeamRepository + ITournamentRepository.
// Mientras los repos reales de Persona 1/2 no estén listos, inyecta
// los Fakes (FakeTeamRepository, FakeTournamentRepository) en Program.cs.
//
// Reglas de negocio a validar en AssignTeamsAsync (-> 422 si fallan):
//   - cada teamId debe existir
//   - el equipo debe pertenecer al mismo torneo que el grupo
//   - el equipo no debe estar ya asignado a otro grupo del torneo
//   - no exceder tournament.Format.MaxTeamsPerGroup
// ============================================================
public class GroupDelegate : IGroupDelegate
{
    private readonly IGroupRepository _groupRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITournamentRepository _tournamentRepository;

    public GroupDelegate(
        IGroupRepository groupRepository,
        ITeamRepository teamRepository,
        ITournamentRepository tournamentRepository)
    {
        _groupRepository = groupRepository;
        _teamRepository = teamRepository;
        _tournamentRepository = tournamentRepository;
    }

    public Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId) => throw new NotImplementedException();
    public Task<Group?> GetByIdAsync(string tournamentId, string groupId) => throw new NotImplementedException();
    public Task<Group?> CreateAsync(string tournamentId, string name) => throw new NotImplementedException();
    public Task<Group?> UpdateAsync(string tournamentId, string groupId, string name) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(string tournamentId, string groupId) => throw new NotImplementedException();

    public Task<bool?> AssignTeamsAsync(string tournamentId, string groupId, IEnumerable<string> teamIds)
        => throw new NotImplementedException();
}
