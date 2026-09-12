namespace TournamentServices.Domain.Enums;

// A este equipo le tocó exclusivamente el formato NFL: no existe
// ROUND_ROBIN en este proyecto. El enum se deja con un solo valor
// para que el tipo siga siendo explícito en Format.Type, en vez de
// quitarlo del todo y perder esa intención en el código.
public enum TournamentType
{
    NFL
}
