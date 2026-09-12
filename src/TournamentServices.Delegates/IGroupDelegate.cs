using TournamentServices.Domain;

namespace TournamentServices.Delegates;

public interface IGroupDelegate
{
    Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId);
    Task<Group?> GetByIdAsync(string tournamentId, string groupId);
    Task<Group?> CreateAsync(string tournamentId, string name);
    Task<Group?> UpdateAsync(string tournamentId, string groupId, string name);
    Task<bool> DeleteAsync(string tournamentId, string groupId);

    /// <returns>null = 404 (torneo/grupo no existe); false = 422 (regla de negocio violada); true = éxito</returns>
    Task<bool?> AssignTeamsAsync(string tournamentId, string groupId, IEnumerable<string> teamIds);
}
