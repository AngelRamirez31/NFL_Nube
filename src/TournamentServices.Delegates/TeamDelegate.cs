using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 1 — Teams
// Implementación real con Result<T>. Es el MODELO que Personas 2, 3 y 4
// copian para Tournament/Group/MatchDelegate.
//
// Regla de "nombre duplicado" -> 400 (Result.Invalid), según la tabla
// "Mínimos por persona" del plan (Teams es distinto a Groups: Groups usa 422).
// ============================================================
public class TeamDelegate : ITeamDelegate
{
    private readonly ITeamRepository _teamRepository;

    public TeamDelegate(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Result<IReadOnlyList<Team>>> GetAllAsync()
    {
        var teams = await _teamRepository.GetAllAsync();
        return Result<IReadOnlyList<Team>>.Ok(teams);
    }

    public async Task<Result<Team>> GetByIdAsync(string id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        return team is null
            ? Result<Team>.NotFound($"Team '{id}' was not found.")
            : Result<Team>.Ok(team);
    }

    public async Task<Result<Team>> CreateAsync(Team team)
    {
        if (string.IsNullOrWhiteSpace(team.Name))
            return Result<Team>.Invalid("Team name is required.");

        if (await _teamRepository.ExistsByNameAsync(team.Name))
            return Result<Team>.Invalid($"A team named '{team.Name}' already exists.");

        try
        {
            var created = await _teamRepository.CreateAsync(team);
            return Result<Team>.Ok(created);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Red de seguridad ante carreras: dos requests casi simultáneos
            // pueden pasar el ExistsByNameAsync antes de que cualquiera inserte.
            // Locust con varios usuarios concurrentes las provoca de verdad.
            return Result<Team>.Invalid($"A team named '{team.Name}' already exists.");
        }
    }

    public async Task<Result<Team>> UpdateAsync(string id, Team team)
    {
        if (string.IsNullOrWhiteSpace(team.Name))
            return Result<Team>.Invalid("Team name is required.");

        var updated = await _teamRepository.UpdateAsync(id, team);
        return updated is null
            ? Result<Team>.NotFound($"Team '{id}' was not found.")
            : Result<Team>.Ok(updated);
    }

    public async Task<Result<Unit>> DeleteAsync(string id)
    {
        var deleted = await _teamRepository.DeleteAsync(id);
        return deleted
            ? Result<Unit>.Ok(Unit.Value)
            : Result<Unit>.NotFound($"Team '{id}' was not found.");
    }
}
