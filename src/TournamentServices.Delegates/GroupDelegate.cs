using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

public class GroupDelegate : IGroupDelegate
{
    private readonly IGroupRepository _groupRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITournamentRepository _tournamentRepository;

    public GroupDelegate(
        IGroupRepository groupRepository,
        ITeamRepository teamRepository,
        ITournamentRepository tournamentRepository)
    {
        _groupRepository = groupRepository;
        _teamRepository = teamRepository;
        _tournamentRepository = tournamentRepository;
    }

    public async Task<Result<IReadOnlyList<Group>>> GetByTournamentAsync(
        string tournamentId)
    {
        var groups = await _groupRepository.GetByTournamentAsync(tournamentId);

        return Result<IReadOnlyList<Group>>.Ok(groups);
    }

    public async Task<Result<Group>> GetByIdAsync(
        string tournamentId,
        string groupId)
    {
        var group = await _groupRepository.GetByIdAsync(
            tournamentId,
            groupId);

        return group is null
            ? Result<Group>.NotFound(
                $"Group '{groupId}' was not found in tournament '{tournamentId}'.")
            : Result<Group>.Ok(group);
    }

    public async Task<Result<Group>> CreateAsync(
        string tournamentId,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Group>.Invalid(
                "Group name is required.");

        var tournament = await _tournamentRepository.GetByIdAsync(
            tournamentId);

        if (tournament is null)
            return Result<Group>.NotFound(
                $"Tournament '{tournamentId}' was not found.");

        if (await _groupRepository.ExistsByNameInTournamentAsync(
                tournamentId,
                name))
        {
            return Result<Group>.Conflict(
                $"A group named '{name}' already exists in tournament '{tournamentId}'.");
        }

        var group = new Group
        {
            Name = name,
            TournamentId = tournament.Id,
            Teams = new List<Team>()
        };

        try
        {
            var created = await _groupRepository.CreateAsync(group);

            return Result<Group>.Ok(created);
        }
        catch (PostgresException ex)
            when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Result<Group>.Conflict(
                $"A group named '{name}' already exists in tournament '{tournamentId}'.");
        }
        catch (PostgresException ex)
            when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return Result<Group>.NotFound(
                $"Tournament '{tournamentId}' was not found.");
        }
    }

    public async Task<Result<Group>> UpdateAsync(
        string tournamentId,
        string groupId,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Group>.Invalid(
                "Group name is required.");

        var group = await _groupRepository.GetByIdAsync(
            tournamentId,
            groupId);

        if (group is null)
        {
            return Result<Group>.NotFound(
                $"Group '{groupId}' was not found in tournament '{tournamentId}'.");
        }

        if (!string.Equals(group.Name, name, StringComparison.Ordinal))
        {
            var duplicate =
                await _groupRepository.ExistsByNameInTournamentAsync(
                    tournamentId,
                    name);

            if (duplicate)
            {
                return Result<Group>.Conflict(
                    $"A group named '{name}' already exists in tournament '{tournamentId}'.");
            }
        }

        group.Name = name;

        try
        {
            var updated = await _groupRepository.UpdateAsync(
                groupId,
                group);

            return updated is null
                ? Result<Group>.NotFound(
                    $"Group '{groupId}' was not found in tournament '{tournamentId}'.")
                : Result<Group>.Ok(updated);
        }
        catch (PostgresException ex)
            when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Result<Group>.Conflict(
                $"A group named '{name}' already exists in tournament '{tournamentId}'.");
        }
    }

    public async Task<Result<Unit>> DeleteAsync(
        string tournamentId,
        string groupId)
    {
        var group = await _groupRepository.GetByIdAsync(
            tournamentId,
            groupId);

        if (group is null)
        {
            return Result<Unit>.NotFound(
                $"Group '{groupId}' was not found in tournament '{tournamentId}'.");
        }

        var deleted = await _groupRepository.DeleteAsync(groupId);

        return deleted
            ? Result<Unit>.Ok(Unit.Value)
            : Result<Unit>.NotFound(
                $"Group '{groupId}' was not found in tournament '{tournamentId}'.");
    }

    public async Task<Result<Unit>> AssignTeamsAsync(
        string tournamentId,
        string groupId,
        IEnumerable<string> teamIds)
    {
        var group = await _groupRepository.GetByIdAsync(
            tournamentId,
            groupId);

        if (group is null)
        {
            return Result<Unit>.NotFound(
                $"Group '{groupId}' was not found in tournament '{tournamentId}'.");
        }

        var requestedTeamIds = teamIds.ToList();

        var normalizedIds = requestedTeamIds
            .Select(NormalizeId)
            .ToList();

        if (normalizedIds.Count !=
            normalizedIds.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            return Result<Unit>.Conflict(
                "The same team cannot be assigned more than once in a single request.");
        }

        var tournament = await _tournamentRepository.GetByIdAsync(
            tournamentId);

        if (tournament is null)
        {
            return Result<Unit>.NotFound(
                $"Tournament '{tournamentId}' was not found.");
        }

        if (group.Teams.Count + requestedTeamIds.Count >
            tournament.Format.MaxTeamsPerGroup)
        {
            return Result<Unit>.Conflict(
                $"Group '{groupId}' cannot contain more than " +
                $"{tournament.Format.MaxTeamsPerGroup} teams.");
        }

        var teamsToAssign = new List<Team>();

        foreach (var teamId in requestedTeamIds)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);

            if (team is null)
            {
                return Result<Unit>.Conflict(
                    $"Team '{teamId}' does not exist.");
            }

            var assignedGroup =
                await _groupRepository.FindByTournamentAndTeamAsync(
                    tournamentId,
                    teamId);

            if (assignedGroup is not null)
            {
                return Result<Unit>.Conflict(
                    $"Team '{teamId}' is already assigned to group " +
                    $"'{assignedGroup.Id}' in tournament '{tournamentId}'.");
            }

            teamsToAssign.Add(team);
        }

        group.Teams.AddRange(teamsToAssign);

        var updated = await _groupRepository.UpdateAsync(
            groupId,
            group);

        if (updated is null)
        {
            return Result<Unit>.NotFound(
                $"Group '{groupId}' was not found in tournament '{tournamentId}'.");
        }

        return Result<Unit>.Ok(Unit.Value);
    }

    private static string NormalizeId(string id)
    {
        if (Guid.TryParse(id, out var parsed))
            return parsed.ToString();

        return id.Trim();
    }
}