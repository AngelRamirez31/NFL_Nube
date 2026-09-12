using TournamentServices.Domain;
using TournamentServices.Repositories;

namespace TournamentServices.Delegates;

// ============================================================
// PERSONA 2 — Tournaments
// TODO: orquesta ITournamentRepository (y en el futuro, cascada a
// Groups/Matches al borrar). En PatchAsync, aplica solo los campos
// no nulos que vengan del PatchTournamentDto.
// ============================================================
public class TournamentDelegate : ITournamentDelegate
{
    private readonly ITournamentRepository _tournamentRepository;

    public TournamentDelegate(ITournamentRepository tournamentRepository)
    {
        _tournamentRepository = tournamentRepository;
    }

    public Task<IReadOnlyList<Tournament>> GetAllAsync() => throw new NotImplementedException();
    public Task<Tournament?> GetByIdAsync(string id) => throw new NotImplementedException();
    public Task<Tournament> CreateAsync(Tournament tournament) => throw new NotImplementedException();
    public Task<Tournament?> UpdateAsync(string id, Tournament tournament) => throw new NotImplementedException();

    public Task<Tournament?> PatchAsync(string id, string? name, int? numberOfGroups, int? maxTeamsPerGroup, string? type)
        => throw new NotImplementedException();

    public Task<bool> DeleteAsync(string id) => throw new NotImplementedException();
}
