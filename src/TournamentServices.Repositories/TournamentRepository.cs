using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// ============================================================
// PERSONA 2 — Tournaments
// TODO: implementar esta clase con una lista/diccionario en memoria.
// Recuerda que Format.Type solo acepta NFL en este proyecto.
// ============================================================
public class TournamentRepository : ITournamentRepository
{
    private readonly List<Tournament> _tournaments = new();

    public Task<IReadOnlyList<Tournament>> GetAllAsync()
    {
        // TODO: PERSONA 2
        throw new NotImplementedException();
    }

    public Task<Tournament?> GetByIdAsync(string id)
    {
        // TODO: PERSONA 2
        throw new NotImplementedException();
    }

    public Task<Tournament> CreateAsync(Tournament tournament)
    {
        // TODO: PERSONA 2 — genera el Id
        throw new NotImplementedException();
    }

    public Task<Tournament?> UpdateAsync(string id, Tournament tournament)
    {
        // TODO: PERSONA 2 — usado tanto por PUT (todos los campos) como por PATCH
        // (en PATCH, aplica el merge en el Delegate, no aquí)
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string id)
    {
        // TODO: PERSONA 2 — recuerda el cascade: borrar también sus Groups y Matches
        throw new NotImplementedException();
    }
}
