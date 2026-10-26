using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

// Reglas del torneo: generar partidos, calcular posiciones, armar playoffs y decidir el campeón.
public static class TournamentRules
{
    // Cuántos equipos de cada grupo pasan a playoffs.
    public const int TeamsPerGroupToPlayoffs = 2;

    // Todos contra todos: cada par de equipos del grupo juega una vez.
    public static List<Match> CreateGroupMatches(string tournamentId, Group group)
    {
        var matches = new List<Match>();

        for (var i = 0; i < group.Teams.Count; i++)
        {
            for (var j = i + 1; j < group.Teams.Count; j++)
            {
                matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    GroupId = group.Id,
                    HomeTeamId = group.Teams[i].Id,
                    VisitorTeamId = group.Teams[j].Id,
                    HomeTeam = group.Teams[i],
                    VisitorTeam = group.Teams[j],
                    Stage = MatchStage.GROUP,
                    Round = 1
                });
            }
        }

        return matches;
    }

    // Calcula la tabla de un grupo usando solo los partidos de grupos que ya tienen marcador.
    public static List<Standing> CalculateStandings(Group group, IEnumerable<Match> matches)
    {
        var table = group.Teams.ToDictionary(t => t.Id, t => new Standing { Team = t });

        foreach (var match in matches)
        {
            if (match.Stage != MatchStage.GROUP || match.Score is null) continue;

            // Ambos equipos deben ser de este grupo.
            if (!table.TryGetValue(match.HomeTeamId, out var home)) continue;
            if (!table.TryGetValue(match.VisitorTeamId, out var visitor)) continue;

            home.Played++;
            visitor.Played++;
            home.PointsFor += match.Score.HomeTeamScore;
            home.PointsAgainst += match.Score.VisitorTeamScore;
            visitor.PointsFor += match.Score.VisitorTeamScore;
            visitor.PointsAgainst += match.Score.HomeTeamScore;

            if (match.Winner == Winner.HOME)
            {
                home.Wins++;
                visitor.Losses++;
            }
            else
            {
                visitor.Wins++;
                home.Losses++;
            }
        }

        // Desempate: más victorias, mejor diferencia de puntos, más puntos anotados y por último el nombre.
        return table.Values
            .OrderByDescending(s => s.Wins)
            .ThenByDescending(s => s.PointDifference)
            .ThenByDescending(s => s.PointsFor)
            .ThenBy(s => s.Team.Name, StringComparer.Ordinal)
            .ToList();
    }

    // Equipos que pasan a playoffs, ordenados del sembrado 1 (el mejor) al último.
    public static List<Team> GetPlayoffSeeds(IEnumerable<Group> groups, IEnumerable<Match> matches)
    {
        var qualified = new List<(Standing Standing, int Position)>();

        foreach (var group in groups)
        {
            var standings = CalculateStandings(group, matches);

            for (var position = 0; position < standings.Count && position < TeamsPerGroupToPlayoffs; position++)
            {
                qualified.Add((standings[position], position));
            }
        }

        // Primero van los líderes de grupo y luego los segundos lugares; entre ellos, el de mejor récord.
        var seeds = qualified
            .OrderBy(q => q.Position)
            .ThenByDescending(q => q.Standing.Wins)
            .ThenByDescending(q => q.Standing.PointDifference)
            .ThenByDescending(q => q.Standing.PointsFor)
            .ThenBy(q => q.Standing.Team.Name, StringComparer.Ordinal)
            .Select(q => q.Standing.Team)
            .ToList();

        // La llave necesita 2, 4, 8... equipos. Si sobran, se quedan fuera los peores sembrados.
        var bracketSize = 1;
        while (bracketSize * 2 <= seeds.Count)
        {
            bracketSize *= 2;
        }

        return seeds.Take(bracketSize).ToList();
    }

    // Arma una ronda de playoffs: el mejor sembrado contra el peor, el segundo contra el penúltimo...
    // El mejor sembrado juega como local.
    public static List<Match> CreatePlayoffRound(string tournamentId, List<Team> seeds, int round)
    {
        var matches = new List<Match>();

        for (var i = 0; i < seeds.Count / 2; i++)
        {
            var home = seeds[i];
            var visitor = seeds[seeds.Count - 1 - i];

            matches.Add(new Match
            {
                TournamentId = tournamentId,
                GroupId = null,
                HomeTeamId = home.Id,
                VisitorTeamId = visitor.Id,
                HomeTeam = home,
                VisitorTeam = visitor,
                Stage = MatchStage.PLAYOFF,
                Round = round
            });
        }

        return matches;
    }

    // Partidos de la última ronda de playoffs generada (lista vacía si aún no hay playoffs).
    public static List<Match> GetLastPlayoffRound(IEnumerable<Match> matches)
    {
        var playoffs = matches.Where(m => m.Stage == MatchStage.PLAYOFF).ToList();
        if (playoffs.Count == 0) return new List<Match>();

        var lastRound = playoffs.Max(m => m.Round);
        return playoffs.Where(m => m.Round == lastRound).ToList();
    }

    // El campeón es el ganador de la final (la ronda de playoffs que tiene un solo partido).
    public static Team? GetChampion(IEnumerable<Group> groups, IEnumerable<Match> matches)
    {
        var lastRound = GetLastPlayoffRound(matches);
        if (lastRound.Count != 1 || !lastRound[0].IsCompleted) return null;

        var championId = lastRound[0].WinnerTeamId;
        return groups.SelectMany(g => g.Teams).FirstOrDefault(t => t.Id == championId);
    }
}
