using TournamentServices.Domain;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 1 — Teams
// TODO: orquesta ITeamRepository. Aquí va la regla de "nombre único".
// ============================================================
public class TeamDelegate : ITeamDelegate
{
    private readonly ITeamRepository _teamRepository;

    public TeamDelegate(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public Task<IReadOnlyList<Team>> GetAllAsync() => throw new NotImplementedException();
    public Task<Team?> GetByIdAsync(string id) => throw new NotImplementedException();
    public Task<Team> CreateAsync(Team team) => throw new NotImplementedException();
    public Task<Team?> UpdateAsync(string id, Team team) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(string id) => throw new NotImplementedException();
}
