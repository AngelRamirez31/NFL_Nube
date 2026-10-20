using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Domain.Enums;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

public class TournamentDelegate : ITournamentDelegate
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMatchRepository _matchRepository;

    public TournamentDelegate(
        ITournamentRepository tournamentRepository,
        IGroupRepository groupRepository,
        IMatchRepository matchRepository)
    {
        _tournamentRepository = tournamentRepository;
        _groupRepository = groupRepository;
        _matchRepository = matchRepository;
    }

    public async Task<Result<IReadOnlyList<Tournament>>> GetAllAsync()
    {
        var tournaments = await _tournamentRepository.GetAllAsync();
        return Result<IReadOnlyList<Tournament>>.Ok(tournaments);
    }

    public async Task<Result<Tournament>> GetByIdAsync(string id)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(id);
        if (tournament is null)
            return Result<Tournament>.NotFound($"Tournament '{id}' was not found.");

        // Cargamos los grupos y partidos del torneo.
        tournament.Groups = (await _groupRepository.GetByTournamentAsync(id)).ToList();
        tournament.Matches = (await _matchRepository.GetByTournamentAsync(id)).ToList();

        // Los equipos de cada partido se toman de los grupos ya cargados.
        var teams = tournament.Groups
            .SelectMany(g => g.Teams)
            .GroupBy(t => t.Id)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var match in tournament.Matches)
        {
            match.HomeTeam = teams.GetValueOrDefault(match.HomeTeamId);
            match.VisitorTeam = teams.GetValueOrDefault(match.VisitorTeamId);
        }

        return Result<Tournament>.Ok(tournament);
    }

    public async Task<Result<Tournament>> CreateAsync(Tournament tournament)
    {
        var invalid = Validate(tournament);
        if (invalid is not null) return invalid;

        try
        {
            var created = await _tournamentRepository.CreateAsync(tournament);
            return Result<Tournament>.Ok(created);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Result<Tournament>.Invalid($"A tournament named '{tournament.Name}' already exists.");
        }
    }

    public async Task<Result<Tournament>> UpdateAsync(string id, Tournament tournament)
    {
        var invalid = Validate(tournament);
        if (invalid is not null) return invalid;

        try
        {
            var updated = await _tournamentRepository.UpdateAsync(id, tournament);
            return updated is null
                ? Result<Tournament>.NotFound($"Tournament '{id}' was not found.")
                : Result<Tournament>.Ok(updated);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Result<Tournament>.Invalid($"A tournament named '{tournament.Name}' already exists.");
        }
    }

    // Se lee el torneo, se cambian solo los campos enviados y se guarda completo.
    public async Task<Result<Tournament>> PatchAsync(string id, string? name, int? numberOfGroups, int? maxTeamsPerGroup, string? type)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(id);
        if (tournament is null)
            return Result<Tournament>.NotFound($"Tournament '{id}' was not found.");

        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Tournament>.Invalid("Tournament name is required.");

            tournament.Name = name;
        }

        if (numberOfGroups is not null)
        {
            if (numberOfGroups <= 0)
                return Result<Tournament>.Invalid("numberOfGroups must be greater than zero.");

            tournament.Format.NumberOfGroups = numberOfGroups.Value;
        }

        if (maxTeamsPerGroup is not null)
        {
            if (maxTeamsPerGroup <= 0)
                return Result<Tournament>.Invalid("maxTeamsPerGroup must be greater than zero.");

            tournament.Format.MaxTeamsPerGroup = maxTeamsPerGroup.Value;
        }

        // Aquí el type llega como texto, así que se valida en este punto.
        if (type is not null)
        {
            if (!Enum.TryParse<TournamentType>(type, ignoreCase: false, out var parsed))
                return Result<Tournament>.Invalid($"'{type}' is not a valid tournament type. Only 'NFL' is supported.");

            tournament.Format.Type = parsed;
        }

        var updated = await _tournamentRepository.UpdateAsync(id, tournament);
        return updated is null
            ? Result<Tournament>.NotFound($"Tournament '{id}' was not found.")
            : Result<Tournament>.Ok(updated);
    }

    // Primero se borran partidos y grupos, y al final el torneo.
    public async Task<Result<Unit>> DeleteAsync(string id)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(id);
        if (tournament is null)
            return Result<Unit>.NotFound($"Tournament '{id}' was not found.");

        foreach (var match in await _matchRepository.GetByTournamentAsync(id))
        {
            await _matchRepository.DeleteAsync(match.Id);
        }

        foreach (var group in await _groupRepository.GetByTournamentAsync(id))
        {
            await _groupRepository.DeleteAsync(group.Id);
        }

        return await _tournamentRepository.DeleteAsync(id)
            ? Result<Unit>.Ok(Unit.Value)
            : Result<Unit>.NotFound($"Tournament '{id}' was not found.");
    }

    private static Result<Tournament>? Validate(Tournament tournament)
    {
        if (string.IsNullOrWhiteSpace(tournament.Name))
            return Result<Tournament>.Invalid("Tournament name is required.");

        if (tournament.Format.NumberOfGroups <= 0)
            return Result<Tournament>.Invalid("numberOfGroups must be greater than zero.");

        if (tournament.Format.MaxTeamsPerGroup <= 0)
            return Result<Tournament>.Invalid("maxTeamsPerGroup must be greater than zero.");

        return null;
    }
}
