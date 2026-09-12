using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

// PERSONA 2 (Tournaments) es la dueña de esta clase.
public class TournamentFormat
{
    public int NumberOfGroups { get; set; } = 1;
    public int MaxTeamsPerGroup { get; set; } = 16;
    public TournamentType Type { get; set; } = TournamentType.NFL;
}
