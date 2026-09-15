using TournamentServices.Domain;
using TournamentServices.Domain.Enums;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates.Tests.Fakes;

public class FakeTournamentRepository : ITournamentRepository
{
    private readonly List<Tournament> _tournaments = new()
    {
        new Tournament
        {
            Id = "tournament-1",
            Name = "NFL Playoffs Demo",
            Format = new TournamentFormat { Type = TournamentType.NFL, NumberOfGroups = 2, MaxTeamsPerGroup = 4 }
        }
    };

    public Task<IReadOnlyList<Tournament>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Tournament>>(_tournaments);

    public Task<Tournament?> GetByIdAsync(string id) =>
        Task.FromResult(_tournaments.FirstOrDefault(t => t.Id == id));

    public Task<Tournament> CreateAsync(Tournament tournament)
    {
        tournament.Id = Guid.NewGuid().ToString();
        _tournaments.Add(tournament);
        return Task.FromResult(tournament);
    }

    public Task<Tournament?> UpdateAsync(string id, Tournament tournament)
    {
        var index = _tournaments.FindIndex(t => t.Id == id);
        if (index < 0) return Task.FromResult<Tournament?>(null);

        tournament.Id = id;
        _tournaments[index] = tournament;
        return Task.FromResult<Tournament?>(tournament);
    }

    public Task<bool> DeleteAsync(string id) => Task.FromResult(_tournaments.RemoveAll(t => t.Id == id) > 0);
}
