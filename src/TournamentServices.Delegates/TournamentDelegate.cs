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

        // El contrato pide que el detalle traiga sus groups y matches, que viven
        // en sus propias tablas.
        tournament.Groups = (await _groupRepository.GetByTournamentAsync(id)).ToList();
        tournament.Matches = (await _matchRepository.GetByTournamentAsync(id)).ToList();

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

    // READ - MODIFY - WRITE: se lee el torneo, se aplican solo los campos que
    // vinieron, y se persiste el documento completo. Un merge superficial de
    // JSONB (document || @patch) borraría el resto de Format.
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

        // A diferencia de Create/Update, aquí el type llega como string suelto,
        // así que la validación del enum tiene que hacerse en esta capa.
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

    // El orden importa: GROUPS tiene FK a TOURNAMENTS, así que borrar el torneo
    // primero fallaría con 23503.
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
