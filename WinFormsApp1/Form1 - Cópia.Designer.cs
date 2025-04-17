namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Labelnome = new Label();
            Labelnumero = new Label();
            txtNome = new TextBox();
            txtNumero = new TextBox();
            btnSalvar = new Button();
            dgvCadastros = new DataGridView();
            npgsqlDataAdapter1 = new Npgsql.NpgsqlDataAdapter();
            ((System.ComponentModel.ISupportInitialize)dgvCadastros).BeginInit();
            SuspendLayout();
            // 
            // Labelnome
            // 
            Labelnome.AutoSize = true;
            Labelnome.Location = new Point(40, 31);
            Labelnome.Name = "Labelnome";
            Labelnome.Size = new Size(43, 15);
            Labelnome.TabIndex = 0;
            Labelnome.Text = "Nome:";
            // 
            // Labelnumero
            // 
            Labelnumero.AutoSize = true;
            Labelnumero.Location = new Point(29, 78);
            Labelnumero.Name = "Labelnumero";
            Labelnumero.Size = new Size(54, 15);
            Labelnumero.TabIndex = 1;
            Labelnumero.Text = "Numero:";
            // 
            // txtNome
            // 
            txtNome.Location = new Point(89, 28);
            txtNome.Name = "txtNome";
            txtNome.Size = new Size(100, 23);
            txtNome.TabIndex = 2;
            // 
            // txtNumero
            // 
            txtNumero.Location = new Point(89, 75);
            txtNumero.Name = "txtNumero";
            txtNumero.Size = new Size(100, 23);
            txtNumero.TabIndex = 3;
            // 
            // btnSalvar
            // 
            btnSalvar.Location = new Point(89, 122);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(75, 23);
            btnSalvar.TabIndex = 4;
            btnSalvar.Text = "Salvar";
            btnSalvar.UseVisualStyleBackColor = true;
            btnSalvar.Click += BtnSalvar_Click;
            // 
            // dgvCadastros
            // 
            dgvCadastros.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCadastros.Location = new Point(104, 173);
            dgvCadastros.Name = "dgvCadastros";
            dgvCadastros.Size = new Size(528, 207);
            dgvCadastros.TabIndex = 5;
            // 
            // npgsqlDataAdapter1
            // 
            npgsqlDataAdapter1.DeleteCommand = null;
            npgsqlDataAdapter1.InsertCommand = null;
            npgsqlDataAdapter1.SelectCommand = null;
            npgsqlDataAdapter1.UpdateCommand = null;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvCadastros);
            Controls.Add(btnSalvar);
            Controls.Add(txtNumero);
            Controls.Add(txtNome);
            Controls.Add(Labelnumero);
            Controls.Add(Labelnome);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvCadastros).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Labelnome;
        private Label Labelnumero;
        private TextBox txtNome;
        private TextBox txtNumero;
        private Button btnSalvar;
        private DataGridView dgvCadastros;
        private Npgsql.NpgsqlDataAdapter npgsqlDataAdapter1;
    }
}
