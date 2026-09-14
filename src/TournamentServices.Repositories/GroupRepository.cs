using Npgsql;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// ============================================================
// PERSONA 3 — Groups
// TODO: implementar contra Postgres siguiendo el patrón de TeamRepository.cs.
// Mientras Teams/Tournaments reales no estén listos, usa los fakes de
// tests/TournamentServices.Delegates.Tests/Fakes/ SOLO en tus propios tests
// (nunca en Program.cs).
//
// SQL confirmado en PostgresConnectionProvider.hpp del profesor — cópialo:
//
//   insert into GROUPS (tournament_id, document) values (@tournamentId, @document::jsonb) returning id;
//   select id, document from GROUPS where tournament_id = @tournamentId;
//   select id, document from GROUPS where tournament_id = @tournamentId and id = @groupId;
//
//   -- ¿este equipo ya está en algún grupo de este torneo? (select_group_in_tournament)
//   select id, document from GROUPS
//   where tournament_id = @tournamentId
//     and document @> jsonb_build_object('teams', jsonb_build_array(jsonb_build_object('id', @teamId::text)));
//
//   -- agregar equipo al arreglo del documento (update_group_add_team)
//   -- OJO: jsonb_insert NO crea la llave si no existe — CreateAsync debe
//   -- escribir siempre "teams": [] o esto falla en silencio.
//   update GROUPS set document = jsonb_insert(document, '{teams,-1}', @team::jsonb),
//                     last_update_date = CURRENT_TIMESTAMP
//   where id = @groupId;
//
// Completar (el C++ los dejó comentados/sin implementar):
//   update GROUPS set document = jsonb_set(document, '{name}', to_jsonb(@name::text)),
//                     last_update_date = CURRENT_TIMESTAMP
//   where tournament_id = @tournamentId and id = @groupId returning id;
//   delete from GROUPS where tournament_id = @tournamentId and id = @groupId;
// ============================================================
public class GroupRepository : IGroupRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public GroupRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public Task<IReadOnlyList<Group>> GetByTournamentAsync(string tournamentId)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }

    public Task<Group?> GetByIdAsync(string tournamentId, string groupId)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }

    public Task<Group> CreateAsync(Group group)
    {
        // TODO: PERSONA 3 — recuerda inicializar "teams": [] en el document
        throw new NotImplementedException();
    }

    public Task<Group?> UpdateAsync(string groupId, Group group)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string groupId)
    {
        // TODO: PERSONA 3
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByNameInTournamentAsync(string tournamentId, string name)
    {
        // TODO: PERSONA 3 — el índice único tournament_group_unique_name_idx
        // también protege esto; captura SqlState 23505 como red de seguridad.
        throw new NotImplementedException();
    }

    public Task<Group?> FindByTournamentAndTeamAsync(string tournamentId, string teamId)
    {
        // TODO: PERSONA 3 — usar select_group_in_tournament (arriba)
        throw new NotImplementedException();
    }
}
