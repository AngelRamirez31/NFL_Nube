using System.Text.Json.Serialization;

namespace TournamentServices.Domain;

// PERSONA 2 (Tournaments) es la dueña de esta clase.
public class Tournament
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TournamentFormat Format { get; set; } = new();

    // Groups y Matches viven en sus propias tablas; el delegate los rellena al
    // leer. [JsonIgnore] evita duplicarlos dentro del JSONB del torneo.
    [JsonIgnore]
    public List<Group> Groups { get; set; } = new();

    [JsonIgnore]
    public List<Match> Matches { get; set; } = new();
}
