# Guía de uso — Tournament Services (NFL)

Cómo levantar la API, correr los tests y simular un torneo NFL completo desde la terminal.
Todos los comandos se corren en **PowerShell** desde la carpeta raíz del proyecto (donde está `TournamentServices.sln`).

## Requisitos

- .NET SDK 10 (`dotnet --version`)
- Podman (para la base de datos PostgreSQL)

---

## 1. Correr la API

### 1.1 Levantar la base de datos

```powershell
podman machine start     # en Windows, enciende la máquina virtual de Podman (si no está encendida)
podman compose up -d
```

Esto crea el contenedor `tournament_db` con PostgreSQL en `localhost:5432` y corre los scripts de `database/` la primera vez.

| Comando | Para qué sirve |
|---|---|
| `podman compose ps` | Ver si la base está corriendo |
| `podman compose logs -f` | Ver los logs de la base (Ctrl+C para salir) |
| `podman compose stop` | Apagar la base (los datos se conservan) |
| `podman compose down -v` | Borrar la base **con todos sus datos** y empezar de cero |

Para ver las tablas con SQL:

```powershell
podman exec -it tournament_db psql -U postgres -d tournament_db
```

Dentro de `psql`: `\dt` lista las tablas, `select document from matches;` muestra los partidos y `\q` sale.

### 1.2 Arrancar la API

```powershell
dotnet restore TournamentServices.sln              # solo la primera vez
dotnet run --project src/TournamentServices.Api
```

La API queda en **http://localhost:8080**.

| URL | Qué es |
|---|---|
| http://localhost:8080/health | Revisa que la API esté viva |
| http://localhost:8080/scalar/v1 | Documentación interactiva (como Swagger), para probar endpoints con clics |
| http://localhost:8080/openapi/v1.json | Especificación OpenAPI (se puede importar en Postman) |

Otros comandos útiles:

```powershell
dotnet watch --project src/TournamentServices.Api   # reinicia la API sola cuando cambias código
dotnet build TournamentServices.sln                 # solo compilar
```

Para detener la API: **Ctrl+C** en la terminal donde corre. Si quedó abierta en otra ventana:

```powershell
Stop-Process -Name TournamentServices.Api
```

> **Importante:** después de cambiar código hay que reiniciar la API (a menos que uses `dotnet watch`).
> Si un `build` o `test` falla con *"el archivo está bloqueado por TournamentServices.Api"*, es porque la API sigue corriendo: detenla y vuelve a intentar.

---

## 2. Tests

Los tests son código C# con **xUnit** (no Postman ni Swagger). Hay un proyecto por capa:

| Proyecto | Qué prueba | Necesita la base |
|---|---|---|
| `tests/TournamentServices.Domain.Tests` | Las reglas de la NFL (`TournamentRules`) | No |
| `tests/TournamentServices.Delegates.Tests` | La lógica de cada caso de uso, con repositorios falsos | No |
| `tests/TournamentServices.Api.Tests` | Los endpoints HTTP, con repositorios falsos | No |
| `tests/TournamentServices.Repositories.Tests` | El acceso a PostgreSQL | **Sí** |

### 2.1 Comandos

```powershell
dotnet test TournamentServices.sln                        # todos los tests
dotnet test tests/TournamentServices.Domain.Tests         # solo un proyecto
dotnet test --filter "FullyQualifiedName~TournamentFlow"  # solo los tests de torneo completo
dotnet test --filter "FullyQualifiedName~WildCard"        # tests cuyo nombre contiene "WildCard"
dotnet test --filter "FullyQualifiedName~RealNflTournament_With32Teams_PlaysLikeTheNfl"   # un solo test
dotnet test --logger "console;verbosity=detailed"         # ver el nombre de cada test al correr
```

### 2.2 Los tests más importantes

**Torneo completo de principio a fin**

| Test | Archivo | Qué demuestra |
|---|---|---|
| `RealNflTournament_With32Teams_PlaysLikeTheNfl` | `Delegates.Tests/TournamentFlowTests.cs` | 32 equipos en 8 divisiones: 48 partidos de grupos → 6 Wild Card → 4 Divisional → 2 Campeonatos de conferencia → Super Bowl → campeón |
| `FullTournament_EndsWithAChampion` | `Delegates.Tests/TournamentFlowTests.cs` | Torneo chico (2 grupos de 3): bye del sembrado 1, campeonatos de conferencia y Super Bowl |
| `FullTournament_GeneratesMatchesAndReturnsTheChampion` | `Api.Tests/TournamentFlowRoutesTests.cs` | El mismo flujo pero por HTTP, llamando a los endpoints reales |

**Reglas de la NFL**

| Test | Archivo | Qué demuestra |
|---|---|---|
| `GetPlayoffSeeds_FourDivisions_DivisionWinnersThenThreeWildCards` | `Domain.Tests/TournamentRulesTests.cs` | Sembrados 1-4 = campeones de división, 5-7 = comodines. Un campeón de división va arriba de un comodín aunque tenga peor récord |
| `CreateNextPlayoffRound_WildCardRound_TopSeedRestsAndTheRestPlay` | `Domain.Tests/TournamentRulesTests.cs` | El 1 descansa y se juega 2 vs 7, 3 vs 6, 4 vs 5 |
| `CreateNextPlayoffRound_DivisionalRound_TopSeedPlaysLowestRemainingSeed` | `Domain.Tests/TournamentRulesTests.cs` | Re-siembra: el 1 juega contra el peor sembrado que sigue vivo |
| `CreateNextPlayoffRound_WhenBothConferencesHaveAChampion_CreatesTheSuperBowl` | `Domain.Tests/TournamentRulesTests.cs` | Campeón AFC vs campeón NFC en el Super Bowl |
| `GetConferences_SplitsGroupsByNameIntoAfcAndNfc` | `Domain.Tests/TournamentRulesTests.cs` | Los grupos se dividen en AFC y NFC por nombre |
| `GetByes_ReturnsHowManyTopSeedsRest` | `Domain.Tests/TournamentRulesTests.cs` | Cuántos sembrados descansan según el número de equipos |
| `CalculateStandings_Tie_CountsAsHalfAWin` | `Domain.Tests/TournamentRulesTests.cs` | Un empate vale media victoria en el PCT: `(G + 0.5 × E) / J` |

**Validaciones del flujo**

| Test | Archivo | Qué demuestra |
|---|---|---|
| `GenerateMatchesAsync_WithMatchesWithoutScore_ReturnsConflict` | `Delegates.Tests/TournamentDelegateTests.cs` | No se avanza de fase si hay partidos sin marcador (422) |
| `GenerateMatchesAsync_WithOnlyOneGroup_ReturnsConflict` | `Delegates.Tests/TournamentDelegateTests.cs` | Se necesitan al menos 2 grupos (uno por conferencia) |
| `UpdateScoreAsync_WhenPlayoffsAlreadyExist_ReturnsConflict` | `Delegates.Tests/MatchDelegateTests.cs` | No se puede cambiar un marcador si ya se generó la siguiente ronda |
| `UpdateScoreAsync_WhenTiedInGroupStage_ReturnsTieAndIsCompleted` | `Delegates.Tests/MatchDelegateTests.cs` | En la fase de grupos se permite el empate (`winner: TIE`) |
| `UpdateScoreAsync_WhenTiedInPlayoffs_ReturnsConflict` | `Delegates.Tests/MatchDelegateTests.cs` | En playoffs y Super Bowl no hay empates (422) |

Para correr solo los de empates:

```powershell
dotnet test --filter "FullyQualifiedName~Tie"
```

Para correr solo los de playoffs:

```powershell
dotnet test --filter "FullyQualifiedName~TournamentFlow|FullyQualifiedName~PlayoffRound|FullyQualifiedName~PlayoffSeeds"
```

> Con xUnit el filtro se hace con `FullyQualifiedName` (`~` = contiene, `=` = igual, `|` = o). `Name` no funciona.

---

## 3. Script de automatización: `scripts/simular-torneo.ps1`

Juega un torneo NFL completo **contra la API que está corriendo** y lo guarda en la base de datos. Sirve para la demo.

### 3.1 Uso

Primero arranca la base y la API (sección 1). Luego, en otra terminal:

```powershell
# Torneo nuevo con marcadores al azar (cada corrida sale distinta)
powershell -ExecutionPolicy Bypass -File scripts/simular-torneo.ps1

# Con semilla: los marcadores salen iguales cada vez (útil para repetir la demo)
powershell -ExecutionPolicy Bypass -File scripts/simular-torneo.ps1 -Semilla 42

# Si la API corre en otro puerto
powershell -ExecutionPolicy Bypass -File scripts/simular-torneo.ps1 -Api http://localhost:5000
```

| Parámetro | Default | Para qué sirve |
|---|---|---|
| `-Api` | `http://localhost:8080` | Dirección de la API |
| `-Semilla` | `0` (al azar) | Si es distinto de 0, repite los mismos marcadores |

`-ExecutionPolicy Bypass` es necesario porque Windows bloquea los scripts `.ps1` por defecto.

### 3.2 Qué hace

1. Revisa que la API responda en `/health`.
2. Usa los **32 equipos reales de la NFL**. La primera vez los crea; en las siguientes corridas los reutiliza (el nombre de un equipo no se puede repetir).
3. Crea un torneo nuevo llamado `Simulacion NFL <fecha y hora>` con 8 grupos de 4.
4. Crea las 8 divisiones reales (`AFC East`, `AFC North`, `AFC South`, `AFC West`, `NFC East`, `NFC North`, `NFC South`, `NFC West`) y les asigna sus equipos.
5. Llama a `POST /tournaments/{id}/matches/generate` y pone marcadores al azar en cada fase, hasta el Super Bowl.
   En la fase de grupos, más o menos 1 de cada 10 partidos queda **empatado**; en playoffs nunca hay empate, como en la NFL.
6. Imprime la tabla de cada división (G-P-E y PCT), quién descansa en el Wild Card, cada partido de playoffs y el campeón.

Cada corrida crea un torneo **nuevo**; los anteriores se quedan en la base.

### 3.3 Ejemplo de salida

Corrida con `-Semilla 7`:

```
Torneo: Simulacion NFL 2026-10-26 20:13:33
Id:     6d79804f-ecec-40d7-aa6e-4d97c92d60d8

=== Fase de grupos ===
48 partidos jugados.

AFC North        G-P-E    PCT
  Bengals      2-0-1   0.833   PF  94   PC  56   Dif   38
  Browns       2-0-1   0.833   PF  63   PC  54   Dif    9
  Ravens       1-2-0   0.333   PF  58   PC  83   Dif  -25
  Steelers     0-3-0   0.000   PF  33   PC  55   Dif  -22
  ...

=== Wild Card ===
Descansan (bye): Raiders, Lions
[AFC] Bengals 38 - 14 Chargers   -> gana Bengals
[AFC] Dolphins 6 - 24 Jaguars   -> gana Jaguars
[AFC] Colts 42 - 35 Browns   -> gana Colts
...

=== Divisional ===
[AFC] Raiders 42 - 31 Jaguars   -> gana Raiders
[AFC] Bengals 30 - 31 Colts   -> gana Colts
...

=== Campeonato de conferencia ===
[AFC] Raiders 35 - 17 Colts   -> gana Raiders
[NFC] Buccaneers 13 - 7 Commanders   -> gana Buccaneers

=== Super Bowl ===
Raiders 21 - 35 Buccaneers   -> gana Buccaneers

CAMPEON DEL SUPER BOWL: Buccaneers
```

En este ejemplo los Browns (0.833) entran como comodín (sembrado 5) y quedan abajo de los Colts (0.667, sembrado 4), porque los Colts ganaron su división. Así funciona en la NFL.

### 3.4 Revisar o borrar un torneo simulado

Con el id que imprime el script:

```powershell
$id = "0daca39a-2f7a-4624-8607-bf4f24987bfd"
Invoke-RestMethod "http://localhost:8080/tournaments/$id/standings" | ConvertTo-Json -Depth 5   # tabla
Invoke-RestMethod "http://localhost:8080/tournaments/$id/matches" | Format-Table stage, round, conference, isCompleted
Invoke-RestMethod "http://localhost:8080/tournaments/$id/champion"                               # campeón
Invoke-RestMethod "http://localhost:8080/tournaments/$id" -Method Delete                         # borrar el torneo
```

Para ver todos los torneos guardados:

```powershell
Invoke-RestMethod http://localhost:8080/tournaments | Format-Table id, name
```

---

## 4. Problemas comunes

| Problema | Solución |
|---|---|
| `La API no responde en http://localhost:8080` | Arranca la API: `dotnet run --project src/TournamentServices.Api` |
| La API no arranca por la conexión a la base | Levanta la base: `podman machine start` y luego `podman compose up -d` |
| `podman compose` dice que no se puede conectar | La máquina de Podman está apagada: `podman machine start` |
| `el archivo está bloqueado por TournamentServices.Api` al compilar o correr tests | Detén la API (Ctrl+C o `Stop-Process -Name TournamentServices.Api`) |
| El script se comporta como la versión vieja | Reinicia la API para que cargue el código nuevo |
| Fallan los tests de `Repositories.Tests` | Esos tests necesitan la base corriendo (`podman compose up -d`) |
| `generate` responde 422 | Hay partidos sin marcador, faltan grupos, o el torneo ya terminó (revisa el mensaje del error) |
| Poner marcador responde 422 en un partido de playoffs | Es un empate: en playoffs no se permite, captura el marcador final con tiempo extra |
