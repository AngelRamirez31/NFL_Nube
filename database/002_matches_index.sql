-- Índice para buscar rápido los partidos de un torneo.
\connect tournament_db tournament_admin

CREATE INDEX IF NOT EXISTS matches_tournament_idx ON MATCHES ((document->>'tournamentId'));
