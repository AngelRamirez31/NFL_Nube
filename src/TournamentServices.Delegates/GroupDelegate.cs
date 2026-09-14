using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 3 — Groups
// TODO: orquesta IGroupRepository + ITeamRepository + ITournamentRepository.
// La lógica de negocio del profesor ya está resuelta en GroupDelegate.hpp/.cpp
// del repo de referencia — traducirla casi literal:
//
//   CreateAsync: 404 si el torneo no existe (Result.NotFound).
//                422 si el nombre del grupo ya existe en el torneo
//                (ExistsByNameInTournamentAsync + capturar SqlState 23505
//                como red de seguridad ante carreras).
//
//   AssignTeamsAsync, en este orden:
//     1) grupo inexistente -> 404
//     2) equipo duplicado en la MISMA petición -> 422
//     3) grupo.Teams.Count + nuevos > tournament.Format.MaxTeamsPerGroup -> 422
//        (el C++ hardcodea >= 32; AQUÍ SE LEE MaxTeamsPerGroup del torneo,
//        que es lo que pide este avance)
//     4) por cada teamId: no existe -> 422 · ya asignado a otro grupo del
//        mismo torneo (FindByTournamentAndTeamAsync) -> 422
//     5) éxito -> 204 (Result<Unit>.Ok)
//
// Mientras Teams/Tournaments reales no estén listos, inyecta los fakes de
// tests/TournamentServices.Delegates.Tests/Fakes/ SOLO en tus propios tests
// (WebApplicationFactory / unit tests con Moq), nunca en Program.cs.
// ============================================================
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

    public Task<Result<IReadOnlyList<Group>>> GetByTournamentAsync(string tournamentId) => throw new NotImplementedException();
    public Task<Result<Group>> GetByIdAsync(string tournamentId, string groupId) => throw new NotImplementedException();
    public Task<Result<Group>> CreateAsync(string tournamentId, string name) => throw new NotImplementedException();
    public Task<Result<Group>> UpdateAsync(string tournamentId, string groupId, string name) => throw new NotImplementedException();
    public Task<Result<Unit>> DeleteAsync(string tournamentId, string groupId) => throw new NotImplementedException();

    public Task<Result<Unit>> AssignTeamsAsync(string tournamentId, string groupId, IEnumerable<string> teamIds)
        => throw new NotImplementedException();
}
