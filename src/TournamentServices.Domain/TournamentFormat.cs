using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

public class TournamentFormat
{
    public int NumberOfGroups { get; set; } = 1;
    public int MaxTeamsPerGroup { get; set; } = 16;
    public TournamentType Type { get; set; } = TournamentType.NFL;
}
