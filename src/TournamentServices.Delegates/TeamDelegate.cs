using Npgsql;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

public class TeamDelegate : ITeamDelegate
{
    private readonly ITeamRepository _teamRepository;

    // Recibe el repositorio de equipos.
    public TeamDelegate(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    // Regresa todos los equipos.
    public async Task<Result<IReadOnlyList<Team>>> GetAllAsync()
    {
        var teams = await _teamRepository.GetAllAsync();
        return Result<IReadOnlyList<Team>>.Ok(teams);
    }

    // Busca un equipo por su id.
    public async Task<Result<Team>> GetByIdAsync(string id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        return team is null
            ? Result<Team>.NotFound($"Team '{id}' was not found.")
            : Result<Team>.Ok(team);
    }

    // Crea un equipo si tiene nombre y no está repetido.
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
            // Por si dos peticiones al mismo tiempo intentan crear el mismo nombre.
            return Result<Team>.Invalid($"A team named '{team.Name}' already exists.");
        }
    }

    // Actualiza un equipo si tiene nombre y existe.
    public async Task<Result<Team>> UpdateAsync(string id, Team team)
    {
        if (string.IsNullOrWhiteSpace(team.Name))
            return Result<Team>.Invalid("Team name is required.");

        var updated = await _teamRepository.UpdateAsync(id, team);
        return updated is null
            ? Result<Team>.NotFound($"Team '{id}' was not found.")
            : Result<Team>.Ok(updated);
    }

    // Borra un equipo por su id.
    public async Task<Result<Unit>> DeleteAsync(string id)
    {
        var deleted = await _teamRepository.DeleteAsync(id);
        return deleted
            ? Result<Unit>.Ok(Unit.Value)
            : Result<Unit>.NotFound($"Team '{id}' was not found.");
    }
}
