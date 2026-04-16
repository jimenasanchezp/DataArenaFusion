namespace DataArenaFusion
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            pnlImportar = new Panel();
            btnGenerarGrafica = new Button();
            btnLimpiar = new Button();
            btnImpPostgre = new Button();
            btnImpMaria = new Button();
            btnImpSql = new Button();
            btnImpTxt = new Button();
            btnImpXml = new Button();
            btnImpJson = new Button();
            btnImpCsv = new Button();
            lblImportar = new Label();
            pnlExportar = new Panel();
            btnExpMaria = new Button();
            btnExpPostgre = new Button();
            lblExportar = new Label();
            tabControlPrincipal = new TabControl();
            tabTabla = new TabPage();
            dgvDatos = new DataGridView();
            pnlFiltro = new Panel();
            lblRegistros = new Label();
            cmbFiltroFuente = new ComboBox();
            lblFiltro = new Label();
            tabGraficas = new TabPage();
            pnlConfigGrafica = new Panel();
            btnGraficar = new Button();
            cmbTipoGrafica = new ComboBox();
            lblTipo = new Label();
            cmbEjeY = new ComboBox();
            lblEjeY = new Label();
            cmbEjeX = new ComboBox();
            lblEjeX = new Label();
            pnlImportar.SuspendLayout();
            pnlExportar.SuspendLayout();
            tabControlPrincipal.SuspendLayout();
            tabTabla.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatos).BeginInit();
            pnlFiltro.SuspendLayout();
            tabGraficas.SuspendLayout();
            pnlConfigGrafica.SuspendLayout();
            SuspendLayout();
            // 
            // pnlImportar
            // 
            pnlImportar.BackColor = Color.FromArgb(32, 34, 40);
            pnlImportar.Controls.Add(btnGenerarGrafica);
            pnlImportar.Controls.Add(btnLimpiar);
            pnlImportar.Controls.Add(btnImpPostgre);
            pnlImportar.Controls.Add(btnImpMaria);
            pnlImportar.Controls.Add(btnImpSql);
            pnlImportar.Controls.Add(btnImpTxt);
            pnlImportar.Controls.Add(btnImpXml);
            pnlImportar.Controls.Add(btnImpJson);
            pnlImportar.Controls.Add(btnImpCsv);
            pnlImportar.Controls.Add(lblImportar);
            pnlImportar.Dock = DockStyle.Top;
            pnlImportar.Location = new Point(0, 0);
            pnlImportar.Name = "pnlImportar";
            pnlImportar.Size = new Size(1184, 55);
            pnlImportar.TabIndex = 0;
            // 
            // btnGenerarGrafica
            // 
            btnGenerarGrafica.BackColor = Color.FromArgb(94, 129, 172);
            btnGenerarGrafica.FlatAppearance.BorderSize = 0;
            btnGenerarGrafica.FlatStyle = FlatStyle.Flat;
            btnGenerarGrafica.ForeColor = Color.White;
            btnGenerarGrafica.Location = new Point(920, 12);
            btnGenerarGrafica.Name = "btnGenerarGrafica";
            btnGenerarGrafica.Size = new Size(140, 30);
            btnGenerarGrafica.TabIndex = 9;
            btnGenerarGrafica.Text = "⚡ Procesar Datos";
            btnGenerarGrafica.UseVisualStyleBackColor = false;
            // 
            // btnLimpiar
            // 
            btnLimpiar.BackColor = Color.FromArgb(191, 97, 106);
            btnLimpiar.FlatAppearance.BorderSize = 0;
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnLimpiar.ForeColor = Color.White;
            btnLimpiar.Location = new Point(820, 12);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(85, 30);
            btnLimpiar.TabIndex = 8;
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.UseVisualStyleBackColor = false;
            // 
            // btnImpPostgre
            // 
            btnImpPostgre.BackColor = Color.FromArgb(76, 86, 106);
            btnImpPostgre.FlatAppearance.BorderSize = 0;
            btnImpPostgre.FlatStyle = FlatStyle.Flat;
            btnImpPostgre.ForeColor = Color.White;
            btnImpPostgre.Location = new Point(660, 12);
            btnImpPostgre.Name = "btnImpPostgre";
            btnImpPostgre.Size = new Size(110, 30);
            btnImpPostgre.TabIndex = 7;
            btnImpPostgre.Text = "PostgreSQL";
            btnImpPostgre.UseVisualStyleBackColor = false;
            // 
            // btnImpMaria
            // 
            btnImpMaria.BackColor = Color.FromArgb(76, 86, 106);
            btnImpMaria.FlatAppearance.BorderSize = 0;
            btnImpMaria.FlatStyle = FlatStyle.Flat;
            btnImpMaria.ForeColor = Color.White;
            btnImpMaria.Location = new Point(560, 12);
            btnImpMaria.Name = "btnImpMaria";
            btnImpMaria.Size = new Size(90, 30);
            btnImpMaria.TabIndex = 6;
            btnImpMaria.Text = "MariaDB";
            btnImpMaria.UseVisualStyleBackColor = false;
            // 
            // btnImpSql
            // 
            btnImpSql.BackColor = Color.FromArgb(76, 86, 106);
            btnImpSql.FlatAppearance.BorderSize = 0;
            btnImpSql.FlatStyle = FlatStyle.Flat;
            btnImpSql.ForeColor = Color.White;
            btnImpSql.Location = new Point(460, 12);
            btnImpSql.Name = "btnImpSql";
            btnImpSql.Size = new Size(90, 30);
            btnImpSql.TabIndex = 5;
            btnImpSql.Text = "SQL Srv";
            btnImpSql.UseVisualStyleBackColor = false;
            // 
            // btnImpTxt
            // 
            btnImpTxt.BackColor = Color.FromArgb(180, 142, 173);
            btnImpTxt.FlatAppearance.BorderSize = 0;
            btnImpTxt.FlatStyle = FlatStyle.Flat;
            btnImpTxt.ForeColor = Color.White;
            btnImpTxt.Location = new Point(380, 12);
            btnImpTxt.Name = "btnImpTxt";
            btnImpTxt.Size = new Size(65, 30);
            btnImpTxt.TabIndex = 4;
            btnImpTxt.Text = "TXT";
            btnImpTxt.UseVisualStyleBackColor = false;
            // 
            // btnImpXml
            // 
            btnImpXml.BackColor = Color.FromArgb(136, 192, 208);
            btnImpXml.FlatAppearance.BorderSize = 0;
            btnImpXml.FlatStyle = FlatStyle.Flat;
            btnImpXml.ForeColor = Color.FromArgb(46, 52, 64);
            btnImpXml.Location = new Point(300, 12);
            btnImpXml.Name = "btnImpXml";
            btnImpXml.Size = new Size(65, 30);
            btnImpXml.TabIndex = 3;
            btnImpXml.Text = "XML";
            btnImpXml.UseVisualStyleBackColor = false;
            // 
            // btnImpJson
            // 
            btnImpJson.BackColor = Color.FromArgb(235, 203, 139);
            btnImpJson.FlatAppearance.BorderSize = 0;
            btnImpJson.FlatStyle = FlatStyle.Flat;
            btnImpJson.ForeColor = Color.FromArgb(46, 52, 64);
            btnImpJson.Location = new Point(220, 12);
            btnImpJson.Name = "btnImpJson";
            btnImpJson.Size = new Size(65, 30);
            btnImpJson.TabIndex = 2;
            btnImpJson.Text = "JSON";
            btnImpJson.UseVisualStyleBackColor = false;
            // 
            // btnImpCsv
            // 
            btnImpCsv.BackColor = Color.FromArgb(163, 190, 140);
            btnImpCsv.FlatAppearance.BorderSize = 0;
            btnImpCsv.FlatStyle = FlatStyle.Flat;
            btnImpCsv.ForeColor = Color.FromArgb(46, 52, 64);
            btnImpCsv.Location = new Point(140, 12);
            btnImpCsv.Name = "btnImpCsv";
            btnImpCsv.Size = new Size(65, 30);
            btnImpCsv.TabIndex = 1;
            btnImpCsv.Text = "CSV";
            btnImpCsv.UseVisualStyleBackColor = false;
            // 
            // lblImportar
            // 
            lblImportar.AutoSize = true;
            lblImportar.Font = new Font("Trebuchet MS", 10.2F, FontStyle.Bold);
            lblImportar.ForeColor = Color.FromArgb(216, 222, 233);
            lblImportar.Location = new Point(15, 16);
            lblImportar.Name = "lblImportar";
            lblImportar.Size = new Size(124, 23);
            lblImportar.TabIndex = 0;
            lblImportar.Text = "📥 IMPORTAR:";
            // 
            // pnlExportar
            // 
            pnlExportar.BackColor = Color.FromArgb(46, 52, 64);
            pnlExportar.Controls.Add(btnExpMaria);
            pnlExportar.Controls.Add(btnExpPostgre);
            pnlExportar.Controls.Add(lblExportar);
            pnlExportar.Dock = DockStyle.Top;
            pnlExportar.Location = new Point(0, 55);
            pnlExportar.Name = "pnlExportar";
            pnlExportar.Size = new Size(1184, 45);
            pnlExportar.TabIndex = 1;
            // 
            // btnExpMaria
            // 
            btnExpMaria.BackColor = Color.FromArgb(67, 76, 94);
            btnExpMaria.FlatAppearance.BorderSize = 0;
            btnExpMaria.FlatStyle = FlatStyle.Flat;
            btnExpMaria.ForeColor = Color.White;
            btnExpMaria.Location = new Point(300, 8);
            btnExpMaria.Name = "btnExpMaria";
            btnExpMaria.Size = new Size(90, 28);
            btnExpMaria.TabIndex = 9;
            btnExpMaria.Text = "MariaDB";
            btnExpMaria.UseVisualStyleBackColor = false;
            // 
            // btnExpPostgre
            // 
            btnExpPostgre.BackColor = Color.FromArgb(67, 76, 94);
            btnExpPostgre.FlatAppearance.BorderSize = 0;
            btnExpPostgre.FlatStyle = FlatStyle.Flat;
            btnExpPostgre.ForeColor = Color.White;
            btnExpPostgre.Location = new Point(200, 8);
            btnExpPostgre.Name = "btnExpPostgre";
            btnExpPostgre.Size = new Size(90, 28);
            btnExpPostgre.TabIndex = 8;
            btnExpPostgre.Text = "SQL Srv";
            btnExpPostgre.UseVisualStyleBackColor = false;
            // 
            // lblExportar
            // 
            lblExportar.AutoSize = true;
            lblExportar.Font = new Font("Trebuchet MS", 10.2F, FontStyle.Bold);
            lblExportar.ForeColor = Color.FromArgb(216, 222, 233);
            lblExportar.Location = new Point(15, 10);
            lblExportar.Name = "lblExportar";
            lblExportar.Size = new Size(165, 23);
            lblExportar.TabIndex = 1;
            lblExportar.Text = "📤 EXPORTAR A BD:";
            // 
            // tabControlPrincipal
            // 
            tabControlPrincipal.Controls.Add(tabTabla);
            tabControlPrincipal.Controls.Add(tabGraficas);
            tabControlPrincipal.Dock = DockStyle.Fill;
            tabControlPrincipal.Font = new Font("Trebuchet MS", 9.5F);
            tabControlPrincipal.Location = new Point(0, 100);
            tabControlPrincipal.Name = "tabControlPrincipal";
            tabControlPrincipal.SelectedIndex = 0;
            tabControlPrincipal.Size = new Size(1184, 561);
            tabControlPrincipal.TabIndex = 2;
            // 
            // tabTabla
            // 
            tabTabla.BackColor = Color.FromArgb(236, 239, 244);
            tabTabla.Controls.Add(dgvDatos);
            tabTabla.Controls.Add(pnlFiltro);
            tabTabla.Location = new Point(4, 31);
            tabTabla.Name = "tabTabla";
            tabTabla.Padding = new Padding(3);
            tabTabla.Size = new Size(1176, 526);
            tabTabla.TabIndex = 0;
            tabTabla.Text = "📋 Tabla de Datos";
            // 
            // dgvDatos
            // 
            dgvDatos.AllowUserToAddRows = false;
            dgvDatos.AllowUserToDeleteRows = false;
            dgvDatos.BackgroundColor = Color.White;
            dgvDatos.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(76, 86, 106);
            dataGridViewCellStyle2.Font = new Font("Trebuchet MS", 9.5F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvDatos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvDatos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDatos.Dock = DockStyle.Fill;
            dgvDatos.EnableHeadersVisualStyles = false;
            dgvDatos.Location = new Point(3, 48);
            dgvDatos.Name = "dgvDatos";
            dgvDatos.ReadOnly = true;
            dgvDatos.RowHeadersWidth = 51;
            dgvDatos.Size = new Size(1170, 475);
            dgvDatos.TabIndex = 1;
            // 
            // pnlFiltro
            // 
            pnlFiltro.BackColor = Color.White;
            pnlFiltro.Controls.Add(lblRegistros);
            pnlFiltro.Controls.Add(cmbFiltroFuente);
            pnlFiltro.Controls.Add(lblFiltro);
            pnlFiltro.Dock = DockStyle.Top;
            pnlFiltro.Location = new Point(3, 3);
            pnlFiltro.Name = "pnlFiltro";
            pnlFiltro.Size = new Size(1170, 45);
            pnlFiltro.TabIndex = 0;
            // 
            // lblRegistros
            // 
            lblRegistros.AutoSize = true;
            lblRegistros.ForeColor = Color.FromArgb(94, 129, 172);
            lblRegistros.Location = new Point(300, 12);
            lblRegistros.Name = "lblRegistros";
            lblRegistros.Size = new Size(84, 22);
            lblRegistros.TabIndex = 2;
            lblRegistros.Text = "0 registros";
            // 
            // cmbFiltroFuente
            // 
            cmbFiltroFuente.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroFuente.FormattingEnabled = true;
            cmbFiltroFuente.Location = new Point(140, 9);
            cmbFiltroFuente.Name = "cmbFiltroFuente";
            cmbFiltroFuente.Size = new Size(140, 30);
            cmbFiltroFuente.TabIndex = 1;
            // 
            // lblFiltro
            // 
            lblFiltro.AutoSize = true;
            lblFiltro.Location = new Point(10, 12);
            lblFiltro.Name = "lblFiltro";
            lblFiltro.Size = new Size(139, 22);
            lblFiltro.TabIndex = 0;
            lblFiltro.Text = "Filtrar por fuente:";
            // 
            // tabGraficas
            // 
            tabGraficas.BackColor = Color.FromArgb(236, 239, 244);
            tabGraficas.Controls.Add(pnlConfigGrafica);
            tabGraficas.Location = new Point(4, 31);
            tabGraficas.Name = "tabGraficas";
            tabGraficas.Padding = new Padding(3);
            tabGraficas.Size = new Size(1176, 526);
            tabGraficas.TabIndex = 1;
            tabGraficas.Text = "📈 Gráficas";
            // 
            // pnlConfigGrafica
            // 
            pnlConfigGrafica.BackColor = Color.White;
            pnlConfigGrafica.Controls.Add(btnGraficar);
            pnlConfigGrafica.Controls.Add(cmbTipoGrafica);
            pnlConfigGrafica.Controls.Add(lblTipo);
            pnlConfigGrafica.Controls.Add(cmbEjeY);
            pnlConfigGrafica.Controls.Add(lblEjeY);
            pnlConfigGrafica.Controls.Add(cmbEjeX);
            pnlConfigGrafica.Controls.Add(lblEjeX);
            pnlConfigGrafica.Dock = DockStyle.Top;
            pnlConfigGrafica.Location = new Point(3, 3);
            pnlConfigGrafica.Name = "pnlConfigGrafica";
            pnlConfigGrafica.Size = new Size(1170, 55);
            pnlConfigGrafica.TabIndex = 0;
            // 
            // btnGraficar
            // 
            btnGraficar.BackColor = Color.FromArgb(180, 142, 173);
            btnGraficar.FlatAppearance.BorderSize = 0;
            btnGraficar.FlatStyle = FlatStyle.Flat;
            btnGraficar.ForeColor = Color.White;
            btnGraficar.Location = new Point(740, 11);
            btnGraficar.Name = "btnGraficar";
            btnGraficar.Size = new Size(110, 32);
            btnGraficar.TabIndex = 10;
            btnGraficar.Text = "📊 Graficar";
            btnGraficar.UseVisualStyleBackColor = false;
            // 
            // cmbTipoGrafica
            // 
            cmbTipoGrafica.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoGrafica.FormattingEnabled = true;
            cmbTipoGrafica.Items.AddRange(new object[] { "Barras", "Pastel", "Dispersión" });
            cmbTipoGrafica.Location = new Point(550, 13);
            cmbTipoGrafica.Name = "cmbTipoGrafica";
            cmbTipoGrafica.Size = new Size(140, 30);
            cmbTipoGrafica.TabIndex = 5;
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Location = new Point(500, 16);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(47, 22);
            lblTipo.TabIndex = 4;
            lblTipo.Text = "Tipo:";
            // 
            // cmbEjeY
            // 
            cmbEjeY.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEjeY.FormattingEnabled = true;
            cmbEjeY.Location = new Point(340, 13);
            cmbEjeY.Name = "cmbEjeY";
            cmbEjeY.Size = new Size(140, 30);
            cmbEjeY.TabIndex = 3;
            // 
            // lblEjeY
            // 
            lblEjeY.AutoSize = true;
            lblEjeY.Location = new Point(260, 16);
            lblEjeY.Name = "lblEjeY";
            lblEjeY.Size = new Size(79, 22);
            lblEjeY.TabIndex = 2;
            lblEjeY.Text = "Eje Y (#):";
            // 
            // cmbEjeX
            // 
            cmbEjeX.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEjeX.FormattingEnabled = true;
            cmbEjeX.Location = new Point(100, 13);
            cmbEjeX.Name = "cmbEjeX";
            cmbEjeX.Size = new Size(140, 30);
            cmbEjeX.TabIndex = 1;
            // 
            // lblEjeX
            // 
            lblEjeX.AutoSize = true;
            lblEjeX.Location = new Point(20, 16);
            lblEjeX.Name = "lblEjeX";
            lblEjeX.Size = new Size(54, 22);
            lblEjeX.TabIndex = 0;
            lblEjeX.Text = "Eje X:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 661);
            Controls.Add(tabControlPrincipal);
            Controls.Add(pnlExportar);
            Controls.Add(pnlImportar);
            Font = new Font("Trebuchet MS", 9F);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Data Arena Fusion";
            pnlImportar.ResumeLayout(false);
            pnlImportar.PerformLayout();
            pnlExportar.ResumeLayout(false);
            pnlExportar.PerformLayout();
            tabControlPrincipal.ResumeLayout(false);
            tabTabla.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDatos).EndInit();
            pnlFiltro.ResumeLayout(false);
            pnlFiltro.PerformLayout();
            tabGraficas.ResumeLayout(false);
            pnlConfigGrafica.ResumeLayout(false);
            pnlConfigGrafica.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlImportar;
        private System.Windows.Forms.Label lblImportar;
        private System.Windows.Forms.Button btnImpCsv;
        private System.Windows.Forms.Button btnImpPostgre;
        private System.Windows.Forms.Button btnImpMaria;
        private System.Windows.Forms.Button btnImpSql;
        private System.Windows.Forms.Button btnImpTxt;
        private System.Windows.Forms.Button btnImpXml;
        private System.Windows.Forms.Button btnImpJson;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnGenerarGrafica;
        private System.Windows.Forms.Panel pnlExportar;
        private System.Windows.Forms.Label lblExportar;
        private System.Windows.Forms.Button btnExpPostgre;
        private System.Windows.Forms.Button btnExpMaria;
        private System.Windows.Forms.Button btnExpPostgre;
        private System.Windows.Forms.TabControl tabControlPrincipal;
        private System.Windows.Forms.TabPage tabTabla;
        private System.Windows.Forms.TabPage tabGraficas;
        private System.Windows.Forms.Panel pnlFiltro;
        private System.Windows.Forms.DataGridView dgvDatos;
        private System.Windows.Forms.Label lblRegistros;
        private System.Windows.Forms.ComboBox cmbFiltroFuente;
        private System.Windows.Forms.Label lblFiltro;
        private System.Windows.Forms.Panel pnlConfigGrafica;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPrincipal;
        private System.Windows.Forms.ComboBox cmbTipoGrafica;
        private System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.ComboBox cmbEjeY;
        private System.Windows.Forms.Label lblEjeY;
        private System.Windows.Forms.ComboBox cmbEjeX;
        private System.Windows.Forms.Label lblEjeX;
        private System.Windows.Forms.Button btnGraficar;
    }
}