using System.Text.Json.Serialization;

namespace TournamentServices.Domain;

public class Match
{
    public string Id { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string HomeTeamId { get; set; } = string.Empty;
    public string VisitorTeamId { get; set; } = string.Empty;

    // Nullable: un match recién creado no tiene score todavía.
    public Score? Score { get; set; }

    // Rellenados por el delegate al leer, para que el MatchDto exponga el
    // nombre del equipo y no solo el id. [JsonIgnore]: nunca se persisten,
    // el documento solo guarda HomeTeamId/VisitorTeamId.
    [JsonIgnore]
    public Team? HomeTeam { get; set; }

    [JsonIgnore]
    public Team? VisitorTeam { get; set; }

    // Calculados, no almacenados: se derivan de Score en vez de guardarse
    // como columnas propias que podrían desincronizarse.
    public bool IsCompleted => Score is not null;
    public Enums.Winner? Winner => Score?.GetWinner();
}
