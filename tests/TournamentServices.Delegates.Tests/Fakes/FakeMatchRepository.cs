using TournamentServices.Domain;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates.Tests.Fakes;

// Doble en memoria de IMatchRepository para tests de rutas y de delegate:
// verifica códigos HTTP y forma del JSON sin levantar Postgres.
// NO es el entregable: el MatchRepository real usa Dapper sobre JSONB.
public class FakeMatchRepository : IMatchRepository
{
    private readonly List<Match> _matches = new();

    public Task<IReadOnlyList<Match>> GetByTournamentAsync(string tournamentId) =>
        Task.FromResult<IReadOnlyList<Match>>(_matches.Where(m => m.TournamentId == tournamentId).ToList());

    public Task<Match?> GetByIdAsync(string tournamentId, string matchId) =>
        Task.FromResult(_matches.FirstOrDefault(m => m.Id == matchId && m.TournamentId == tournamentId));

    public Task<Match> CreateAsync(Match match)
    {
        match.Id = Guid.NewGuid().ToString();
        _matches.Add(match);
        return Task.FromResult(match);
    }

    public Task<Match?> UpdateAsync(string matchId, Match match)
    {
        var index = _matches.FindIndex(m => m.Id == matchId);
        if (index < 0) return Task.FromResult<Match?>(null);

        match.Id = matchId;
        _matches[index] = match;
        return Task.FromResult<Match?>(match);
    }

    public Task<bool> DeleteAsync(string matchId) => Task.FromResult(_matches.RemoveAll(m => m.Id == matchId) > 0);
}
