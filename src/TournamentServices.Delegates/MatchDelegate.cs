using TournamentServices.Domain;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 4 — Matches
// TODO: orquesta IMatchRepository + ITeamRepository + IGroupRepository.
// Mientras los repos reales de Persona 1/3 no estén listos, inyecta
// los Fakes (FakeTeamRepository, FakeGroupRepository) en Program.cs.
//
// Reglas de negocio a validar en CreateAsync (-> 422 si fallan):
//   - homeTeamId y visitorTeamId deben existir y pertenecer al torneo
//   - homeTeamId != visitorTeamId
//   - si se manda groupId, ambos equipos deben estar en ese grupo
//
// En UpdateScoreAsync: score.GetWinner() calcula el ganador,
// marca IsCompleted = true. Scores negativos se validan en el DTO (400),
// no aquí.
// ============================================================
public class MatchDelegate : IMatchDelegate
{
    private readonly IMatchRepository _matchRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IGroupRepository _groupRepository;

    public MatchDelegate(
        IMatchRepository matchRepository,
        ITeamRepository teamRepository,
        IGroupRepository groupRepository)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _groupRepository = groupRepository;
    }

    public Task<IReadOnlyList<Match>> GetByTournamentAsync(string tournamentId) => throw new NotImplementedException();
    public Task<Match?> GetByIdAsync(string tournamentId, string matchId) => throw new NotImplementedException();

    public Task<Match?> CreateAsync(string tournamentId, string? groupId, string homeTeamId, string visitorTeamId)
        => throw new NotImplementedException();

    public Task<Match?> UpdateScoreAsync(string tournamentId, string matchId, int homeScore, int visitorScore)
        => throw new NotImplementedException();

    public Task<bool> DeleteAsync(string tournamentId, string matchId) => throw new NotImplementedException();
}
