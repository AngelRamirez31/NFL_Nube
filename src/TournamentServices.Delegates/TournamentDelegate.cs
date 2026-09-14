using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 2 — Tournaments
// TODO: orquesta ITournamentRepository (+ IGroupRepository/IMatchRepository
// para armar Groups/Matches del TournamentDto, y para el cascade delete).
//
// Reglas:
//   - type solo acepta "NFL" -> Result.Invalid (400) con cualquier otro valor.
//     El enum de un solo valor + JsonStringEnumConverter ya rechaza basura
//     a nivel de deserialización; aquí confirmas con un mensaje claro.
//   - MaxTeamsPerGroup y NumberOfGroups > 0 -> Result.Invalid (400).
//   - PatchAsync es READ-MODIFY-WRITE: lee el Tournament actual, aplica solo
//     los parámetros != null (uno por uno, incluidos los de Format), y
//     persiste el documento completo. NO uses un merge superficial de JSONB
//     (document || @patch) porque sobreescribiría Format entero.
//   - DeleteAsync: cascade matches -> groups -> tournament. Mientras
//     Personas 3/4 no terminan, borrar solo el torneo y dejar el TODO;
//     se cierra en la integración final.
// ============================================================
public class TournamentDelegate : ITournamentDelegate
{
    private readonly ITournamentRepository _tournamentRepository;

    public TournamentDelegate(ITournamentRepository tournamentRepository)
    {
        _tournamentRepository = tournamentRepository;
    }

    public Task<Result<IReadOnlyList<Tournament>>> GetAllAsync() => throw new NotImplementedException();
    public Task<Result<Tournament>> GetByIdAsync(string id) => throw new NotImplementedException();
    public Task<Result<Tournament>> CreateAsync(Tournament tournament) => throw new NotImplementedException();
    public Task<Result<Tournament>> UpdateAsync(string id, Tournament tournament) => throw new NotImplementedException();

    public Task<Result<Tournament>> PatchAsync(string id, string? name, int? numberOfGroups, int? maxTeamsPerGroup, string? type)
        => throw new NotImplementedException();

    public Task<Result<Unit>> DeleteAsync(string id) => throw new NotImplementedException();
}
