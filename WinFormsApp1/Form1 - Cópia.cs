using Npgsql;
using System.Data;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        string connStr = "Host=localhost;Username=cadastro_user;Password=1234;Database=Cadastro";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CarregarDados();
        }
        private void CarregarDados()
        {
            using (var conn = new NpgsqlConnection(connStr))
            {
                var dt = new DataTable();
                var da = new NpgsqlDataAdapter("SELECT * FROM cadastro ORDER BY id", conn);
                da.Fill(dt);
                dgvCadastros.DataSource = dt;
            }
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            using (var conn = new NpgsqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    var sql = "INSERT INTO cadastro (nome, numero) VALUES (@nome, @numero)";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("nome", txtNome.Text);
                        cmd.Parameters.AddWithValue("numero", int.Parse(txtNumero.Text));
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Salvo com sucesso!");
                    txtNome.Clear();
                    txtNumero.Clear();
                    CarregarDados();
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
}