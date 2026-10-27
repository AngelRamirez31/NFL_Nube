using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

// Reglas del torneo estilo NFL: fase de grupos (divisiones), playoffs por conferencia y Super Bowl.
public static class TournamentRules
{
    // Como en la NFL: 7 equipos por conferencia (campeones de división + comodines).
    public const int PlayoffTeamsPerConference = 7;

    public const string Afc = "AFC";
    public const string Nfc = "NFC";

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
            else if (match.Winner == Winner.VISITOR)
            {
                visitor.Wins++;
                home.Losses++;
            }
            else
            {
                home.Ties++;
                visitor.Ties++;
            }
        }

        return OrderByRecord(table.Values);
    }

    // Divide los grupos en las 2 conferencias. Los grupos se ordenan por nombre:
    // la primera mitad es la AFC y la segunda la NFC (si es impar, la AFC tiene uno más).
    public static List<Conference> GetConferences(IEnumerable<Group> groups)
    {
        var ordered = groups.OrderBy(g => g.Name, StringComparer.Ordinal).ToList();
        var afcCount = (ordered.Count + 1) / 2;

        return new List<Conference>
        {
            new Conference { Name = Afc, Groups = ordered.Take(afcCount).ToList() },
            new Conference { Name = Nfc, Groups = ordered.Skip(afcCount).ToList() }
        };
    }

    // Sembrados de una conferencia, del 1 al 7.
    // Primero van los campeones de división (el 1° de cada grupo), ordenados por récord.
    // Después los comodines (wild cards): los mejores récords de los demás equipos, sin importar su grupo.
    public static List<Team> GetPlayoffSeeds(IEnumerable<Group> conferenceGroups, IEnumerable<Match> matches)
    {
        var divisionWinners = new List<Standing>();
        var others = new List<Standing>();

        foreach (var group in conferenceGroups)
        {
            var standings = CalculateStandings(group, matches);
            if (standings.Count == 0) continue;

            divisionWinners.Add(standings[0]);
            others.AddRange(standings.Skip(1));
        }

        // Con 4 divisiones quedan 3 comodines, como en la NFL.
        var wildCards = Math.Max(0, PlayoffTeamsPerConference - divisionWinners.Count);

        return OrderByRecord(divisionWinners)
            .Concat(OrderByRecord(others).Take(wildCards))
            .Select(s => s.Team)
            .ToList();
    }

    // Cuántos de los mejores sembrados descansan en la primera ronda (bye).
    // Con 7 equipos la llave es de 8, así que solo descansa el sembrado 1.
    public static int GetByes(int seedCount)
    {
        var bracketSize = 1;
        while (bracketSize < seedCount)
        {
            bracketSize *= 2;
        }

        return bracketSize - seedCount;
    }

    // Arma una ronda de playoffs: el mejor sembrado contra el peor, el segundo contra el penúltimo...
    // El mejor sembrado juega como local.
    public static List<Match> CreatePlayoffRound(string tournamentId, List<Team> seeds, int round, string conference)
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
                Round = round,
                Conference = conference
            });
        }

        return matches;
    }

    // Genera la siguiente ronda de playoffs. Cada conferencia juega su propia llave:
    // Wild Card -> Divisional -> Campeonato de conferencia. Cuando la AFC y la NFC ya tienen
    // campeón, se genera el Super Bowl. Devuelve una lista vacía si ya no hay nada que generar.
    public static List<Match> CreateNextPlayoffRound(string tournamentId, IEnumerable<Group> groups, IEnumerable<Match> matches)
    {
        var playoffs = matches.Where(m => m.Stage != MatchStage.GROUP).ToList();
        if (playoffs.Any(m => m.Stage == MatchStage.SUPER_BOWL)) return new List<Match>();

        var round = playoffs.Count == 0 ? 1 : playoffs.Max(m => m.Round) + 1;
        var newMatches = new List<Match>();
        var champions = new List<Team>();

        foreach (var conference in GetConferences(groups))
        {
            var seeds = GetPlayoffSeeds(conference.Groups, matches);
            var conferenceMatches = playoffs.Where(m => m.Conference == conference.Name).ToList();

            // Siguen vivos los que no han perdido. Quedan en orden de sembrado, así que
            // en cada ronda se vuelve a sembrar: el mejor que queda juega contra el peor que queda.
            var alive = seeds.Where(t => !conferenceMatches.Any(m => m.LoserTeamId == t.Id)).ToList();

            if (alive.Count == 1)
            {
                champions.Add(alive[0]);
                continue;
            }

            // En la primera ronda (Wild Card) los mejores sembrados descansan.
            var teamsThatPlay = conferenceMatches.Count == 0
                ? alive.Skip(GetByes(alive.Count)).ToList()
                : alive;

            newMatches.AddRange(CreatePlayoffRound(tournamentId, teamsThatPlay, round, conference.Name));
        }

        if (newMatches.Count == 0 && champions.Count == 2)
        {
            newMatches.Add(CreateSuperBowl(tournamentId, groups, matches, champions, round));
        }

        return newMatches;
    }

    // El campeón es el ganador del Super Bowl.
    public static Team? GetChampion(IEnumerable<Group> groups, IEnumerable<Match> matches)
    {
        var superBowl = matches.FirstOrDefault(m => m.Stage == MatchStage.SUPER_BOWL);
        if (superBowl is null || !superBowl.IsCompleted) return null;

        return groups.SelectMany(g => g.Teams).FirstOrDefault(t => t.Id == superBowl.WinnerTeamId);
    }

    // Super Bowl: campeón de la AFC contra campeón de la NFC.
    // Se juega en campo neutral; como local se pone al de mejor récord en la fase de grupos.
    private static Match CreateSuperBowl(string tournamentId, IEnumerable<Group> groups, IEnumerable<Match> matches, List<Team> champions, int round)
    {
        var records = groups
            .SelectMany(g => CalculateStandings(g, matches))
            .Where(s => champions.Any(c => c.Id == s.Team.Id));

        var finalists = OrderByRecord(records).Select(s => s.Team).ToList();

        return new Match
        {
            TournamentId = tournamentId,
            GroupId = null,
            HomeTeamId = finalists[0].Id,
            VisitorTeamId = finalists[1].Id,
            HomeTeam = finalists[0],
            VisitorTeam = finalists[1],
            Stage = MatchStage.SUPER_BOWL,
            Round = round,
            Conference = null
        };
    }

    // Ordena por récord: mejor porcentaje de victorias, más victorias, mejor diferencia de puntos,
    // más puntos anotados y por último el nombre.
    private static List<Standing> OrderByRecord(IEnumerable<Standing> standings) =>
        standings
            .OrderByDescending(s => s.WinPercentage)
            .ThenByDescending(s => s.Wins)
            .ThenByDescending(s => s.PointDifference)
            .ThenByDescending(s => s.PointsFor)
            .ThenBy(s => s.Team.Name, StringComparer.Ordinal)
            .ToList();
}
