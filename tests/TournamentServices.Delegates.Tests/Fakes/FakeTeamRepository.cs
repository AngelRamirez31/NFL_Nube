using TournamentServices.Domain;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates.Tests.Fakes;

// Fake precargado para que PERSONA 3 y PERSONA 4 puedan probar
// Groups/Matches sin esperar a que el TeamRepository real esté listo.
// NO es el entregable final: cuando el TeamRepository real de
// PERSONA 1 esté listo, se reemplaza este fake en Program.cs.
public class FakeTeamRepository : ITeamRepository
{
    private readonly List<Team> _teams = new()
    {
        new Team { Id = "team-1", Name = "Eagles" },
        new Team { Id = "team-2", Name = "Cowboys" },
        new Team { Id = "team-3", Name = "Giants" },
    };

    public Task<IReadOnlyList<Team>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Team>>(_teams);

    public Task<Team?> GetByIdAsync(string id) =>
        Task.FromResult(_teams.FirstOrDefault(t => t.Id == id));

    public Task<Team> CreateAsync(Team team)
    {
        team.Id = Guid.NewGuid().ToString();
        _teams.Add(team);
        return Task.FromResult(team);
    }

    public Task<Team?> UpdateAsync(string id, Team team) =>
        Task.FromResult<Team?>(null);

    public Task<bool> DeleteAsync(string id) => Task.FromResult(false);

    public Task<bool> ExistsByNameAsync(string name) =>
        Task.FromResult(_teams.Any(t => t.Name == name));
}
