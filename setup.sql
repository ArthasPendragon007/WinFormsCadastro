CREATE TABLE IF NOT EXISTS cadastro (
    id SERIAL PRIMARY KEY,
    nome TEXT NOT NULL,
    numero INTEGER NOT NULL UNIQUE CHECK (numero > 0)
);

CREATE TABLE IF NOT EXISTS log_operacoes (
    id SERIAL PRIMARY KEY,
    data_hora TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    operacao TEXT NOT NULL CHECK (operacao IN ('INSERT', 'UPDATE', 'DELETE')),
    id_cadastro INTEGER
);

CREATE OR REPLACE FUNCTION registrar_log()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO log_operacoes (operacao, id_cadastro) VALUES ('INSERT', NEW.id);
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO log_operacoes (operacao, id_cadastro) VALUES ('UPDATE', NEW.id);
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO log_operacoes (operacao, id_cadastro) VALUES ('DELETE', OLD.id);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_log_insert') THEN
        CREATE TRIGGER trg_log_insert
        AFTER INSERT ON cadastro
        FOR EACH ROW
        EXECUTE FUNCTION registrar_log();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_log_update') THEN
        CREATE TRIGGER trg_log_update
        AFTER UPDATE ON cadastro
        FOR EACH ROW
        EXECUTE FUNCTION registrar_log();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_log_delete') THEN
        CREATE TRIGGER trg_log_delete
        AFTER DELETE ON cadastro
        FOR EACH ROW
        EXECUTE FUNCTION registrar_log();
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'cadastro_user') THEN
        CREATE USER cadastro_user WITH PASSWORD '1234';
    END IF;
END $$;

GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE cadastro TO cadastro_user;
GRANT USAGE, SELECT ON SEQUENCE cadastro_id_seq TO cadastro_user;
GRANT INSERT ON TABLE log_operacoes TO cadastro_user;
GRANT USAGE, SELECT ON SEQUENCE log_operacoes_id_seq TO cadastro_user;
ALTER TABLE log_operacoes OWNER TO cadastro_user;