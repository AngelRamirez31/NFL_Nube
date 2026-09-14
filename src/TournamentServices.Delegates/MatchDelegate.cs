using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 4 — Matches
// TODO: orquesta IMatchRepository + ITeamRepository + IGroupRepository.
//
// Reglas (-> Result.Conflict = 422, salvo donde se indique):
//   - homeTeamId == visitorTeamId -> 422
//   - homeTeamId/visitorTeamId no existen o no pertenecen al torneo -> 422
//   - si viene groupId, ambos equipos deben estar en ese grupo -> 422
//   - match inexistente -> 404 (Result.NotFound)
//   - score negativo o no entero -> 400 (Result.Invalid) — puede validarse
//     antes de llegar aquí con FluentValidation, pero igual repítelo aquí
//     como defensa en profundidad.
//
// UpdateScoreAsync: construye un nuevo Score, persiste, y el Winner/IsCompleted
// de Match ya se calculan solos (son propiedades computadas en Domain/Match.cs).
//
// Mientras Teams/Groups reales no estén listos, usa los fakes de
// tests/TournamentServices.Delegates.Tests/Fakes/ SOLO en tus propios tests.
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

    public Task<Result<IReadOnlyList<Match>>> GetByTournamentAsync(string tournamentId) => throw new NotImplementedException();
    public Task<Result<Match>> GetByIdAsync(string tournamentId, string matchId) => throw new NotImplementedException();

    public Task<Result<Match>> CreateAsync(string tournamentId, string? groupId, string homeTeamId, string visitorTeamId)
        => throw new NotImplementedException();

    public Task<Result<Match>> UpdateScoreAsync(string tournamentId, string matchId, int homeScore, int visitorScore)
        => throw new NotImplementedException();

    public Task<Result<Unit>> DeleteAsync(string tournamentId, string matchId) => throw new NotImplementedException();
}
