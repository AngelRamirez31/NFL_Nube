namespace TournamentServices.Domain;

// PERSONA 4 (Matches) es la dueña de esta clase.
public class Match
{
    public string Id { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string HomeTeamId { get; set; } = string.Empty;
    public string VisitorTeamId { get; set; } = string.Empty;

    // Nullable: un match recién creado no tiene score todavía.
    public Score? Score { get; set; }

    // Calculados, no almacenados: se derivan de Score en vez de guardarse
    // como columnas propias que podrían desincronizarse.
    public bool IsCompleted => Score is not null;
    public Enums.Winner? Winner => Score?.GetWinner();
}
