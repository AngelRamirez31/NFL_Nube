using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

// Configuración del torneo: cuántos grupos, máximo de equipos por grupo y tipo.
public class TournamentFormat
{
    public int NumberOfGroups { get; set; } = 1;
    public int MaxTeamsPerGroup { get; set; } = 16;
    public TournamentType Type { get; set; } = TournamentType.NFL;
}
