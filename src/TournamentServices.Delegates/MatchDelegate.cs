using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Domain.Enums;
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

        var homeTeam = await _teamRepository.GetByIdAsync(homeTeamId);
        if (homeTeam is null)
        {
            return Result<Match>.Conflict($"Team {homeTeamId} doesn't exist");
        }

        var visitorTeam = await _teamRepository.GetByIdAsync(visitorTeamId);
        if (visitorTeam is null)
        {
            return Result<Match>.Conflict($"Team {visitorTeamId} doesn't exist");
        }

        if (groupId is not null)
        {
            // Si viene groupId, ambos equipos deben estar en ese grupo.
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
            // Si no viene groupId, cada equipo debe estar en algún grupo del torneo.
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

        // Reutilizamos los equipos que ya se buscaron arriba.
        created.HomeTeam = homeTeam;
        created.VisitorTeam = visitorTeam;
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

        // Si ya se generó la siguiente fase, el marcador ya no se puede cambiar.
        var matches = await _matchRepository.GetByTournamentAsync(tournamentId);
        var nextPhaseExists = match.Stage == MatchStage.GROUP
            ? matches.Any(m => m.Stage != MatchStage.GROUP)
            : matches.Any(m => m.Stage != MatchStage.GROUP && m.Round > match.Round);

        if (nextPhaseExists)
        {
            return Result<Match>.Conflict("The score can't be changed because the next round was already generated");
        }

        // Solo se asigna el Score; Winner e IsCompleted se calculan solos.
        match.Score = new Score { HomeTeamScore = homeScore, VisitorTeamScore = visitorScore };

        var updated = await _matchRepository.UpdateAsync(matchId, match);
        if (updated is null)
        {
            return Result<Match>.NotFound($"Match {matchId} not found in tournament {tournamentId}");
        }

        await HydrateTeamsAsync(updated);
        return Result<Match>.Ok(updated);
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

    // Agrega el nombre de los equipos al partido (en la base solo se guardan los ids).
    private async Task HydrateTeamsAsync(Match match)
    {
        match.HomeTeam = await _teamRepository.GetByIdAsync(match.HomeTeamId);
        match.VisitorTeam = await _teamRepository.GetByIdAsync(match.VisitorTeamId);
    }
}
