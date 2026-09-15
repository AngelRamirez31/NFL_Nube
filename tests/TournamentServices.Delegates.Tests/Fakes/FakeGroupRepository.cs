using TournamentServices.Domain;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates.Tests.Fakes;

public class FakeGroupRepository : IGroupRepository
{
    private readonly List<Group> _groups = new()
    {
        new Group
        {
            Id = "group-1",
            Name = "Group A",
            TournamentId = "tournament-1",
            Teams = new List<Team>()
        },

        new Group
        {
            Id = "group-with-teams",
            Name = "Group B",
            TournamentId = "tournament-1",
            Teams = new List<Team>
            {
                new Team { Id = "team-1", Name = "Eagles" },
                new Team { Id = "team-2", Name = "Cowboys" }
            }
        }
    };

    public Task<IReadOnlyList<Group>> GetByTournamentAsync(
        string tournamentId)
    {
        IReadOnlyList<Group> groups = _groups
            .Where(g => g.TournamentId == tournamentId)
            .ToList();

        return Task.FromResult(groups);
    }

    public Task<Group?> GetByIdAsync(
        string tournamentId,
        string groupId)
    {
        var group = _groups.FirstOrDefault(g =>
            g.Id == groupId &&
            g.TournamentId == tournamentId);

        return Task.FromResult(group);
    }

    public Task<Group> CreateAsync(Group group)
    {
        group.Id = Guid.NewGuid().ToString();
        group.Teams ??= new List<Team>();

        _groups.Add(group);

        return Task.FromResult(group);
    }

    public Task<Group?> UpdateAsync(
        string groupId,
        Group group)
    {
        var index = _groups.FindIndex(g => g.Id == groupId);

        if (index < 0)
            return Task.FromResult<Group?>(null);

        group.Id = groupId;
        group.Teams ??= new List<Team>();

        _groups[index] = group;

        return Task.FromResult<Group?>(group);
    }

    public Task<bool> DeleteAsync(string groupId)
    {
        var group = _groups.FirstOrDefault(g => g.Id == groupId);

        if (group is null)
            return Task.FromResult(false);

        _groups.Remove(group);

        return Task.FromResult(true);
    }

    public Task<bool> ExistsByNameInTournamentAsync(
        string tournamentId,
        string name)
    {
        var exists = _groups.Any(g =>
            g.TournamentId == tournamentId &&
            g.Name == name);

        return Task.FromResult(exists);
    }

    public Task<Group?> FindByTournamentAndTeamAsync(
        string tournamentId,
        string teamId)
    {
        var group = _groups.FirstOrDefault(g =>
            g.TournamentId == tournamentId &&
            g.Teams.Any(t => t.Id == teamId));

        return Task.FromResult(group);
    }
}