using TournamentServices.Domain;

namespace TournamentServices.Repositories.Fakes;

// Fake para que PERSONA 4 pueda probar Matches
// sin esperar al GroupRepository real de PERSONA 3.
public class FakeGroupRepository : IGroupRepository
{
    private readonly List<Group> _groups = new()
    {
        new Group { Id = "group-1", Name = "Group A", TournamentId = "tournament-1" }
    };

    public Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId) =>
        Task.FromResult<IReadOnlyList<Group>>(_groups.Where(g => g.TournamentId == tournamentId).ToList());

    public Task<Group?> GetByIdAsync(string tournamentId, string groupId) =>
        Task.FromResult(_groups.FirstOrDefault(g => g.Id == groupId && g.TournamentId == tournamentId));

    public Task<Group> CreateAsync(Group group)
    {
        group.Id = Guid.NewGuid().ToString();
        _groups.Add(group);
        return Task.FromResult(group);
    }

    public Task<Group?> UpdateAsync(string groupId, Group group) => Task.FromResult<Group?>(null);

    public Task<bool> DeleteAsync(string groupId) => Task.FromResult(false);

    public Task<bool> ExistsByNameInTournamentAsync(string tournamentId, string name) =>
        Task.FromResult(_groups.Any(g => g.TournamentId == tournamentId && g.Name == name));
}
