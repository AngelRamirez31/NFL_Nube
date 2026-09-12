using System.Text.RegularExpressions;

namespace TournamentServices.Api.Common;

// Compartido por todas las rutas: valida el formato de {id} del contrato ([A-Za-z0-9-]+).
public static partial class Ids
{
    private static readonly Regex Pattern = new("^[A-Za-z0-9-]+$", RegexOptions.Compiled);

    public static bool IsValid(string id) => !string.IsNullOrWhiteSpace(id) && Pattern.IsMatch(id);
}
