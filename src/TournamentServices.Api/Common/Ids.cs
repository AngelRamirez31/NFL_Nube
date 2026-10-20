using System.Text.RegularExpressions;

namespace TournamentServices.Api.Common;

// Valida que el {id} solo tenga letras, números y guiones.
public static partial class Ids
{
    private static readonly Regex Pattern = new("^[A-Za-z0-9-]+$", RegexOptions.Compiled);

    public static bool IsValid(string id) => !string.IsNullOrWhiteSpace(id) && Pattern.IsMatch(id);
}
