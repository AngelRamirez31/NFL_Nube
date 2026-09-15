using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;
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

    public async Task<Result<IReadOnlyList<Match>>> GetByTournamentAsync(string tournamentId)
    {
        var matches = await _matchRepository.GetByTournamentAsync(tournamentId);

        foreach (var match in matches)
        {
            await HydrateTeamsAsync(match);
        }

        return Result<IReadOnlyList<Match>>.Ok(matches);
    }

    public async Task<Result<Match>> GetByIdAsync(string tournamentId, string matchId)
    {
        var match = await _matchRepository.GetByIdAsync(tournamentId, matchId);
        if (match is null)
        {
            return Result<Match>.NotFound($"Match {matchId} not found in tournament {tournamentId}");
        }

        await HydrateTeamsAsync(match);
        return Result<Match>.Ok(match);
    }

    public async Task<Result<Match>> CreateAsync(string tournamentId, string? groupId, string homeTeamId, string visitorTeamId)
    {
        if (string.Equals(homeTeamId, visitorTeamId, StringComparison.Ordinal))
        {
            return Result<Match>.Conflict("Home and visitor team must be different");
        }

        if (await _teamRepository.GetByIdAsync(homeTeamId) is null)
        {
            return Result<Match>.Conflict($"Team {homeTeamId} doesn't exist");
        }

        if (await _teamRepository.GetByIdAsync(visitorTeamId) is null)
        {
            return Result<Match>.Conflict($"Team {visitorTeamId} doesn't exist");
        }

        if (groupId is not null)
        {
            // Con groupId explícito: el grupo debe existir en el torneo y
            // ambos equipos deben estar en ese mismo grupo.
            var group = await _groupRepository.GetByIdAsync(tournamentId, groupId);
            if (group is null)
            {
                return Result<Match>.Conflict($"Group {groupId} doesn't belong to tournament {tournamentId}");
            }

            if (!group.Teams.Any(t => t.Id == homeTeamId) || !group.Teams.Any(t => t.Id == visitorTeamId))
            {
                return Result<Match>.Conflict($"Both teams must belong to group {groupId}");
            }
        }
        else
        {
            // Sin groupId: basta con que cada equipo esté en algún grupo del torneo.
            if (await _groupRepository.FindByTournamentAndTeamAsync(tournamentId, homeTeamId) is null)
            {
                return Result<Match>.Conflict($"Team {homeTeamId} doesn't belong to tournament {tournamentId}");
            }

            if (await _groupRepository.FindByTournamentAndTeamAsync(tournamentId, visitorTeamId) is null)
            {
                return Result<Match>.Conflict($"Team {visitorTeamId} doesn't belong to tournament {tournamentId}");
            }
        }

        var created = await _matchRepository.CreateAsync(new Match
        {
            TournamentId = tournamentId,
            GroupId = groupId,
            HomeTeamId = homeTeamId,
            VisitorTeamId = visitorTeamId,
            Score = null   // aún no se juega: IsCompleted queda en false
        });

        return Result<Match>.Ok(created);
    }

    public async Task<Result<Match>> UpdateScoreAsync(string tournamentId, string matchId, int homeScore, int visitorScore)
    {
        if (homeScore < 0 || visitorScore < 0)
        {
            return Result<Match>.Invalid("Scores must be zero or positive");
        }

        var match = await _matchRepository.GetByIdAsync(tournamentId, matchId);
        if (match is null)
        {
            return Result<Match>.NotFound($"Match {matchId} not found in tournament {tournamentId}");
        }

        // Asignar el Score es lo único que hace falta: Winner e IsCompleted
        // son propiedades calculadas de Match.
        match.Score = new Score { HomeTeamScore = homeScore, VisitorTeamScore = visitorScore };

        var updated = await _matchRepository.UpdateAsync(matchId, match);

        return updated is null
            ? Result<Match>.NotFound($"Match {matchId} not found in tournament {tournamentId}")
            : Result<Match>.Ok(updated);
    }

    public async Task<Result<Unit>> DeleteAsync(string tournamentId, string matchId)
    {
        var match = await _matchRepository.GetByIdAsync(tournamentId, matchId);
        if (match is null)
        {
            return Result<Unit>.NotFound($"Match {matchId} not found in tournament {tournamentId}");
        }

        return await _matchRepository.DeleteAsync(matchId)
            ? Result<Unit>.Ok(Unit.Value)
            : Result<Unit>.NotFound($"Match {matchId} not found in tournament {tournamentId}");
    }

    // El contrato pide que el MatchDto traiga homeTeam/visitorTeam con su nombre,
    // no solo los ids. El documento JSONB guarda únicamente los ids, así que el
    // detalle se resuelve aquí contra ITeamRepository.
    private async Task HydrateTeamsAsync(Match match)
    {
        match.HomeTeam = await _teamRepository.GetByIdAsync(match.HomeTeamId);
        match.VisitorTeam = await _teamRepository.GetByIdAsync(match.VisitorTeamId);
    }
}
