# Simula un torneo NFL completo (32 equipos, 8 divisiones) contra la API que esta corriendo.
# Cada vez que se corre crea un torneo nuevo con marcadores al azar.
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File scripts/simular-torneo.ps1
#   powershell -ExecutionPolicy Bypass -File scripts/simular-torneo.ps1 -Semilla 42
#   powershell -ExecutionPolicy Bypass -File scripts/simular-torneo.ps1 -Api http://localhost:5000
param(
    [string]$Api = "http://localhost:8080",
    [int]$Semilla = 0
)

$ErrorActionPreference = "Stop"
$json = "application/json"

# Con semilla, los marcadores salen iguales en cada corrida.
if ($Semilla -ne 0) { Get-Random -SetSeed $Semilla | Out-Null }

# Divisiones reales de la NFL con sus equipos.
$divisiones = [ordered]@{
    "AFC East"  = @("Bills", "Dolphins", "Patriots", "Jets")
    "AFC North" = @("Ravens", "Bengals", "Browns", "Steelers")
    "AFC South" = @("Texans", "Colts", "Jaguars", "Titans")
    "AFC West"  = @("Broncos", "Chiefs", "Raiders", "Chargers")
    "NFC East"  = @("Cowboys", "Giants", "Eagles", "Commanders")
    "NFC North" = @("Bears", "Lions", "Packers", "Vikings")
    "NFC South" = @("Falcons", "Panthers", "Saints", "Buccaneers")
    "NFC West"  = @("Cardinals", "Rams", "49ers", "Seahawks")
}

# Marcador al azar parecido a uno de futbol americano.
# En fase de grupos a veces sale empate (1 de cada 10); en playoffs nunca, como en la NFL.
function Get-MarcadorAlAzar([bool]$permitirEmpate) {
    $puntos = @(0, 3, 6, 7, 10, 13, 14, 17, 20, 21, 23, 24, 27, 28, 30, 31, 34, 35, 38, 42)
    $local = Get-Random -InputObject $puntos

    if ($permitirEmpate -and (Get-Random -Maximum 10) -eq 0) {
        return @{ homeTeamScore = $local; visitorTeamScore = $local }
    }

    $visitante = Get-Random -InputObject $puntos
    while ($visitante -eq $local) { $visitante = Get-Random -InputObject $puntos }

    return @{ homeTeamScore = $local; visitorTeamScore = $visitante }
}

# Nombre de la fase segun la etapa y la ronda del partido.
function Get-NombreFase($partido) {
    if ($partido.stage -eq "GROUP") { return "Fase de grupos" }
    if ($partido.stage -eq "SUPER_BOWL") { return "Super Bowl" }

    switch ($partido.round) {
        1 { return "Wild Card" }
        2 { return "Divisional" }
        default { return "Campeonato de conferencia" }
    }
}

# Revisa que la API este corriendo.
try { Invoke-RestMethod "$Api/health" | Out-Null }
catch {
    Write-Host "La API no responde en $Api. Arrancala con: dotnet run --project src/TournamentServices.Api" -ForegroundColor Red
    exit 1
}

# 1) Equipos: se reutilizan si ya existen, porque el nombre no se puede repetir.
$existentes = @{}
# (En PowerShell 5.1 un arreglo JSON llega como un solo objeto; ForEach-Object lo separa en elementos).
foreach ($equipo in @(Invoke-RestMethod "$Api/teams" | ForEach-Object { $_ })) { $existentes[$equipo.name] = $equipo.id }

$nombres = @{}          # id -> nombre, para imprimir los partidos
$idsPorDivision = @{}

foreach ($division in $divisiones.Keys) {
    $ids = @()
    foreach ($nombre in $divisiones[$division]) {
        if ($existentes.ContainsKey($nombre)) {
            $id = $existentes[$nombre]
        }
        else {
            $id = (Invoke-RestMethod "$Api/teams" -Method Post -ContentType $json -Body (@{ name = $nombre } | ConvertTo-Json)).id
        }

        $nombres[$id] = $nombre
        $ids += $id
    }
    $idsPorDivision[$division] = $ids
}

# 2) Torneo nuevo con 8 grupos de 4, como la NFL.
$body = @{
    name   = "Simulacion NFL " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    format = @{ numberOfGroups = 8; maxTeamsPerGroup = 4; type = "NFL" }
} | ConvertTo-Json

$torneo = Invoke-RestMethod "$Api/tournaments" -Method Post -ContentType $json -Body $body
$base = "$Api/tournaments/$($torneo.id)"

Write-Host "Torneo: $($torneo.name)" -ForegroundColor Green
Write-Host "Id:     $($torneo.id)"

# 3) Grupos (divisiones) con sus equipos.
foreach ($division in $divisiones.Keys) {
    $grupo = Invoke-RestMethod "$base/groups" -Method Post -ContentType $json -Body (@{ name = $division } | ConvertTo-Json)
    $equipos = @($idsPorDivision[$division] | ForEach-Object { @{ id = $_ } })

    Invoke-RestMethod "$base/groups/$($grupo.id)/teams" -Method Patch -ContentType $json -Body (ConvertTo-Json -InputObject $equipos) | Out-Null
}

# 4) Se genera y se juega cada fase hasta el Super Bowl.
while ($true) {
    $partidos = @(Invoke-RestMethod "$base/matches/generate" -Method Post | ForEach-Object { $_ })
    $fase = Get-NombreFase $partidos[0]

    Write-Host "`n=== $fase ===" -ForegroundColor Cyan

    # En el Wild Card el sembrado 1 de cada conferencia descansa.
    if ($fase -eq "Wild Card") {
        $jugando = $partidos | ForEach-Object { $_.homeTeamId; $_.visitorTeamId }
        $lideres = $tablas | ForEach-Object { $_.standings[0].team }
        $descansan = $lideres | Where-Object { $jugando -notcontains $_.id } | ForEach-Object { $_.name }
        Write-Host "Descansan (bye): $($descansan -join ', ')"
    }

    foreach ($partido in $partidos) {
        $marcador = Get-MarcadorAlAzar ($partido.stage -eq "GROUP")
        $jugado = Invoke-RestMethod "$base/matches/$($partido.id)/score" -Method Patch -ContentType $json -Body ($marcador | ConvertTo-Json)

        # Los 48 partidos de grupos no se imprimen; se muestra la tabla al final de la fase.
        if ($partido.stage -ne "GROUP") {
            $conferencia = if ($jugado.conference) { "[$($jugado.conference)] " } else { "" }
            Write-Host ("{0}{1} {2} - {3} {4}   -> gana {5}" -f $conferencia,
                $nombres[$jugado.homeTeamId], $jugado.score.homeTeamScore,
                $jugado.score.visitorTeamScore, $nombres[$jugado.visitorTeamId],
                $nombres[$jugado.winnerTeamId])
        }
    }

    # Tabla de posiciones al terminar la fase de grupos.
    if ($fase -eq "Fase de grupos") {
        Write-Host "$($partidos.Count) partidos jugados."
        $tablas = @(Invoke-RestMethod "$base/standings" | ForEach-Object { $_ })

        foreach ($tabla in $tablas) {
            Write-Host "`n$($tabla.groupName)        G-P-E    PCT" -ForegroundColor Yellow
            foreach ($fila in $tabla.standings) {
                Write-Host ("  {0,-12} {1}-{2}-{3}   {4:0.000}   PF {5,3}   PC {6,3}   Dif {7,4}" -f $fila.team.name,
                    $fila.wins, $fila.losses, $fila.ties, $fila.winPercentage,
                    $fila.pointsFor, $fila.pointsAgainst, $fila.pointDifference)
            }
        }
    }

    if ($fase -eq "Super Bowl") { break }
}

# 5) Campeon.
$campeon = Invoke-RestMethod "$base/champion"
Write-Host "`nCAMPEON DEL SUPER BOWL: $($campeon.name)" -ForegroundColor Green
Write-Host "Id del torneo: $($torneo.id)"
