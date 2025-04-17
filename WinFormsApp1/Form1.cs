using Npgsql;
using System.Data;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly string connStr;

        public Form1()
        {
            InitializeComponent();

            // Configurar string de conexão principal usando variáveis de ambiente
            string host = ObterVariavelAmbiente("POSTGRES_HOST", "localhost");
            string user = ObterVariavelAmbiente("POSTGRES_USER", "cadastro_user");
            string password = ObterVariavelAmbiente("POSTGRES_PASSWORD", "1234");
            string database = ObterVariavelAmbiente("POSTGRES_DATABASE", "cadastro");

            connStr = $"Host={host};Username={user};Password={password};Database={database}";
        }

        private string ObterVariavelAmbiente(string nome, string valorPadrao)
        {
            return Environment.GetEnvironmentVariable(nome) ?? valorPadrao;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CriarEstruturaDoBanco();
            CarregarDados();
        }

        private void CriarEstruturaDoBanco()
        {
            // Strings de conexão usando variáveis de ambiente
            string host = ObterVariavelAmbiente("POSTGRES_HOST", "localhost");
            string adminUser = ObterVariavelAmbiente("POSTGRES_ADMIN_USER", "postgres");
            string adminPassword = ObterVariavelAmbiente("POSTGRES_ADMIN_PASSWORD", "1234");
            string appUser = ObterVariavelAmbiente("POSTGRES_USER", "cadastro_user");
            string appPassword = ObterVariavelAmbiente("POSTGRES_PASSWORD", "1234");
            string database = ObterVariavelAmbiente("POSTGRES_DATABASE", "cadastro");

            string connStrPostgres = $"Host={host};Username={adminUser};Password={adminPassword};Database=postgres";
            string connStrCadastro = $"Host={host};Username={adminUser};Password={adminPassword};Database={database}";

            using (var conn = new NpgsqlConnection(connStrPostgres))
            {
                try
                {
                    conn.Open();

                    string verificarBanco = $"SELECT 1 FROM pg_database WHERE datname = '{database}'";
                    using (var cmd = new NpgsqlCommand(verificarBanco, conn))
                    {
                        if (cmd.ExecuteScalar() == null)
                        {
                            new NpgsqlCommand($"CREATE DATABASE {database}", conn).ExecuteNonQuery();
                            MessageBox.Show("Banco de dados criado com sucesso!");
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao criar banco de dados: {ex.Message}");
                }
            }

            using (var conn = new NpgsqlConnection(connStrCadastro))
            {
                try
                {
                    conn.Open();

                    // Tabelas
                    new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS cadastro (
                    id SERIAL PRIMARY KEY,
                    nome TEXT NOT NULL,
                    numero INTEGER NOT NULL UNIQUE CHECK (numero > 0)
                );", conn).ExecuteNonQuery();

                    new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS log_operacoes (
                    id SERIAL PRIMARY KEY,
                    data_hora TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    operacao TEXT NOT NULL CHECK (operacao IN ('INSERT', 'UPDATE', 'DELETE')),
                    id_cadastro INTEGER
                );", conn).ExecuteNonQuery();

                    // logs
                    new NpgsqlCommand(@"
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
                $$ LANGUAGE plpgsql SECURITY DEFINER;", conn).ExecuteNonQuery();

                    // triggers
                    new NpgsqlCommand(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_trigger WHERE tgname = 'trg_log_insert'
                    ) THEN
                        CREATE TRIGGER trg_log_insert
                        AFTER INSERT ON cadastro
                        FOR EACH ROW
                        EXECUTE FUNCTION registrar_log();
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_trigger WHERE tgname = 'trg_log_update'
                    ) THEN
                        CREATE TRIGGER trg_log_update
                        AFTER UPDATE ON cadastro
                        FOR EACH ROW
                        EXECUTE FUNCTION registrar_log();
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_trigger WHERE tgname = 'trg_log_delete'
                    ) THEN
                        CREATE TRIGGER trg_log_delete
                        AFTER DELETE ON cadastro
                        FOR EACH ROW
                        EXECUTE FUNCTION registrar_log();
                    END IF;
                END $$;", conn).ExecuteNonQuery();

                    // usuário e permissões
                    new NpgsqlCommand($"DO $$ BEGIN IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '{appUser}') THEN CREATE USER {appUser} WITH PASSWORD '{appPassword}'; END IF; END $$;", conn).ExecuteNonQuery();

                    new NpgsqlCommand($"GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE cadastro TO {appUser};", conn).ExecuteNonQuery();
                    new NpgsqlCommand($"GRANT USAGE, SELECT ON SEQUENCE cadastro_id_seq TO {appUser};", conn).ExecuteNonQuery();
                    new NpgsqlCommand($"GRANT USAGE, SELECT ON SEQUENCE log_operacoes_id_seq TO {appUser};", conn).ExecuteNonQuery();
                    new NpgsqlCommand($"ALTER TABLE log_operacoes OWNER TO {appUser};", conn).ExecuteNonQuery();

                    MessageBox.Show("Estrutura do banco, triggers e permissões criadas com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao criar estrutura: " + ex.Message);
                }
            }
        }

        private bool VerificarConexao()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar ao banco de dados: {ex.Message}");
                return false;
            }
        }

        private void CarregarDados()
        {
            if (!VerificarConexao())
                return;

            try
            {
                using (var conn = new NpgsqlConnection(connStr))
                {
                    var dt = new DataTable();
                    var da = new NpgsqlDataAdapter("SELECT * FROM cadastro ORDER BY id", conn);
                    da.Fill(dt);
                    dgvCadastros.DataSource = dt;
                    AdicionarBotoesAoGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}");
            }
        }

        private void AdicionarBotoesAoGrid()
        {
            if (!dgvCadastros.Columns.Contains("btnEditar"))
            {
                var btnEditar = new DataGridViewButtonColumn
                {
                    HeaderText = "Editar",
                    Text = "Editar",
                    UseColumnTextForButtonValue = true,
                    Name = "btnEditar"
                };
                dgvCadastros.Columns.Add(btnEditar);
            }

            if (!dgvCadastros.Columns.Contains("btnExcluir"))
            {
                var btnExcluir = new DataGridViewButtonColumn
                {
                    HeaderText = "Excluir",
                    Text = "Excluir",
                    UseColumnTextForButtonValue = true,
                    Name = "btnExcluir"
                };
                dgvCadastros.Columns.Add(btnExcluir);
            }
        }

        private void dgvCadastros_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    var id = Convert.ToInt32(dgvCadastros.Rows[e.RowIndex].Cells["id"].Value);

                    if (dgvCadastros.Columns[e.ColumnIndex].Name == "btnEditar")
                    {
                        txtNome.Text = dgvCadastros.Rows[e.RowIndex].Cells["nome"].Value.ToString();
                        txtNumero.Text = dgvCadastros.Rows[e.RowIndex].Cells["numero"].Value.ToString();
                        btnSalvar.Tag = id;
                    }
                    else if (dgvCadastros.Columns[e.ColumnIndex].Name == "btnExcluir")
                    {
                        if (MessageBox.Show("Deseja realmente excluir?", "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            ExcluirCadastro(id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao clicar na célula: {ex.Message}");
            }
        }

        private void ExcluirCadastro(int id)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    var sql = "DELETE FROM cadastro WHERE id = @id";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                CarregarDados();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir cadastro: {ex.Message}");
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    NpgsqlCommand cmd;

                    if (btnSalvar.Tag == null)
                    {
                        cmd = new NpgsqlCommand("INSERT INTO cadastro (nome, numero) VALUES (@nome, @numero)", conn);
                    }
                    else
                    {
                        cmd = new NpgsqlCommand("UPDATE cadastro SET nome = @nome, numero = @numero WHERE id = @id", conn);
                        cmd.Parameters.AddWithValue("id", (int)btnSalvar.Tag);
                    }

                    cmd.Parameters.AddWithValue("nome", txtNome.Text);
                    cmd.Parameters.AddWithValue("numero", int.Parse(txtNumero.Text));
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Salvo com sucesso!");
                    txtNome.Clear();
                    txtNumero.Clear();
                    btnSalvar.Tag = null;

                    CarregarDados();
                }
            }
            catch (PostgresException pgEx)
            {
                MessageBox.Show("Erro do banco: " + pgEx.MessageText);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }
    }
}

