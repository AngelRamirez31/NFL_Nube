-- Aditivo al db_script.sql del profesor: MATCHES no tiene columna
-- tournament_id (se filtra por el JSONB), así que el filtro por torneo
-- necesita un índice funcional para no hacer table scan.
\connect tournament_db tournament_admin

CREATE INDEX IF NOT EXISTS matches_tournament_idx ON MATCHES ((document->>'tournamentId'));
