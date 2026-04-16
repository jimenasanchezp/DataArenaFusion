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
            lblTitulo = new Label();
            pnlSeparador = new Panel();
            lblImportar = new Label();
            btnImpCsv = new Button();
            btnImpJson = new Button();
            btnImpXml = new Button();
            btnImpTxt = new Button();
            lblExportar = new Label();
            btnImpMaria = new Button();
            btnImpPostgre = new Button();
            btnExpMaria = new Button();
            btnExpPostgre = new Button();
            lblAcciones = new Label();
            btnLimpiar = new Button();
            btnConsola = new Button();
            btnApi = new Button();
            tabControlPrincipal = new TabControl();
            tabTabla = new TabPage();
            dgvDatos = new DataGridView();
            pnlFiltro = new Panel();
            lblRegistros = new Label();
            lblFiltro = new Label();
            cmbFiltroColumna = new ComboBox();
            btnOrdenar = new Button();
            btnAgrupar = new Button();
            btnDuplicados = new Button();
            tabGraficas = new TabPage();
            pnlConfigGrafica = new Panel();
            lblEjeX = new Label();
            cmbEjeX = new ComboBox();
            lblEjeY = new Label();
            cmbEjeY = new ComboBox();
            lblTipo = new Label();
            cmbTipoGrafica = new ComboBox();
          //  btnGraficar = new Button();
            chartPrincipal = new System.Windows.Forms.DataVisualization.Charting.Chart();
            pnlImportar.SuspendLayout();
            tabControlPrincipal.SuspendLayout();
            tabTabla.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatos).BeginInit();
            pnlFiltro.SuspendLayout();
            tabGraficas.SuspendLayout();
            pnlConfigGrafica.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartPrincipal).BeginInit();
            SuspendLayout();
            // 
            // pnlImportar
            // 
            pnlImportar.BackColor = Color.FromArgb(17, 24, 39);
            pnlImportar.Controls.Add(lblTitulo);
            pnlImportar.Controls.Add(pnlSeparador);
            pnlImportar.Controls.Add(lblImportar);
            pnlImportar.Controls.Add(btnImpCsv);
            pnlImportar.Controls.Add(btnImpJson);
            pnlImportar.Controls.Add(btnImpXml);
            pnlImportar.Controls.Add(btnImpTxt);
            pnlImportar.Controls.Add(lblExportar);
            pnlImportar.Controls.Add(btnImpMaria);
            pnlImportar.Controls.Add(btnImpPostgre);
            pnlImportar.Controls.Add(btnExpMaria);
            pnlImportar.Controls.Add(btnExpPostgre);
            pnlImportar.Controls.Add(lblAcciones);
            pnlImportar.Controls.Add(btnLimpiar);
            pnlImportar.Controls.Add(btnApi);
            pnlImportar.Controls.Add(btnConsola);
            pnlImportar.Dock = DockStyle.Left;
            pnlImportar.Location = new Point(0, 0);
            pnlImportar.Name = "pnlImportar";
            pnlImportar.Size = new Size(240, 700);
            pnlImportar.TabIndex = 0;
            // 
            // lblTitulo  (titulo principal del sidebar)
            // 
            lblTitulo.AutoSize = false;
            lblTitulo.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(255, 46, 115);
            lblTitulo.Location = new Point(0, 5);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(240, 45);
            lblTitulo.TabIndex = 99;
            lblTitulo.Text = "Data Arena Fusion";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlSeparador (linea divisora debajo del titulo)
            // 
            pnlSeparador.BackColor = Color.FromArgb(45, 55, 72);
            pnlSeparador.Location = new Point(15, 52);
            pnlSeparador.Name = "pnlSeparador";
            pnlSeparador.Size = new Size(210, 1);
            pnlSeparador.TabIndex = 98;
            // 
            // -- SECCION: IMPORTAR ARCHIVOS --
            // 
            // lblImportar
            // 
            lblImportar.AutoSize = true;
            lblImportar.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblImportar.ForeColor = Color.FromArgb(156, 163, 175);
            lblImportar.Location = new Point(15, 64);
            lblImportar.Name = "lblImportar";
            lblImportar.TabIndex = 0;
            lblImportar.Text = "IMPORTAR ARCHIVOS";
            // 
            // btnImpCsv
            // 
            btnImpCsv.BackColor = Color.FromArgb(163, 190, 140);
            btnImpCsv.FlatAppearance.BorderSize = 0;
            btnImpCsv.FlatStyle = FlatStyle.Flat;
            btnImpCsv.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnImpCsv.ForeColor = Color.FromArgb(46, 52, 64);
            btnImpCsv.Location = new Point(15, 88);
            btnImpCsv.Name = "btnImpCsv";
            btnImpCsv.Size = new Size(210, 36);
            btnImpCsv.TabIndex = 1;
            btnImpCsv.Text = "Importar CSV";
            btnImpCsv.UseVisualStyleBackColor = false;
            // 
            // btnImpJson
            // 
            btnImpJson.BackColor = Color.FromArgb(235, 203, 139);
            btnImpJson.FlatAppearance.BorderSize = 0;
            btnImpJson.FlatStyle = FlatStyle.Flat;
            btnImpJson.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnImpJson.ForeColor = Color.FromArgb(46, 52, 64);
            btnImpJson.Location = new Point(15, 131);
            btnImpJson.Name = "btnImpJson";
            btnImpJson.Size = new Size(210, 36);
            btnImpJson.TabIndex = 2;
            btnImpJson.Text = "Importar JSON";
            btnImpJson.UseVisualStyleBackColor = false;
            // 
            // btnImpXml
            // 
            btnImpXml.BackColor = Color.FromArgb(136, 192, 208);
            btnImpXml.FlatAppearance.BorderSize = 0;
            btnImpXml.FlatStyle = FlatStyle.Flat;
            btnImpXml.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnImpXml.ForeColor = Color.FromArgb(46, 52, 64);
            btnImpXml.Location = new Point(15, 174);
            btnImpXml.Name = "btnImpXml";
            btnImpXml.Size = new Size(210, 36);
            btnImpXml.TabIndex = 3;
            btnImpXml.Text = "Importar XML";
            btnImpXml.UseVisualStyleBackColor = false;
            // 
            // btnImpTxt
            // 
            btnImpTxt.BackColor = Color.FromArgb(180, 142, 173);
            btnImpTxt.FlatAppearance.BorderSize = 0;
            btnImpTxt.FlatStyle = FlatStyle.Flat;
            btnImpTxt.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnImpTxt.ForeColor = Color.White;
            btnImpTxt.Location = new Point(15, 217);
            btnImpTxt.Name = "btnImpTxt";
            btnImpTxt.Size = new Size(210, 36);
            btnImpTxt.TabIndex = 4;
            btnImpTxt.Text = "Importar TXT";
            btnImpTxt.UseVisualStyleBackColor = false;
            // 
            // -- SECCION: BASES DE DATOS --
            // 
            // lblExportar
            // 
            lblExportar.AutoSize = true;
            lblExportar.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblExportar.ForeColor = Color.FromArgb(156, 163, 175);
            lblExportar.Location = new Point(15, 272);
            lblExportar.Name = "lblExportar";
            lblExportar.TabIndex = 1;
            lblExportar.Text = "BASES DE DATOS";
            // 
            // btnImpMaria  (Configurar MariaDB)
            // 
            btnImpMaria.BackColor = Color.FromArgb(76, 86, 106);
            btnImpMaria.FlatAppearance.BorderSize = 0;
            btnImpMaria.FlatStyle = FlatStyle.Flat;
            btnImpMaria.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnImpMaria.ForeColor = Color.White;
            btnImpMaria.Location = new Point(15, 296);
            btnImpMaria.Name = "btnImpMaria";
            btnImpMaria.Size = new Size(210, 36);
            btnImpMaria.TabIndex = 5;
            btnImpMaria.Text = "Configurar MariaDB";
            btnImpMaria.UseVisualStyleBackColor = false;
            // 
            // btnImpPostgre  (Configurar PostgreSQL)
            // 
            btnImpPostgre.BackColor = Color.FromArgb(76, 86, 106);
            btnImpPostgre.FlatAppearance.BorderSize = 0;
            btnImpPostgre.FlatStyle = FlatStyle.Flat;
            btnImpPostgre.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnImpPostgre.ForeColor = Color.White;
            btnImpPostgre.Location = new Point(15, 339);
            btnImpPostgre.Name = "btnImpPostgre";
            btnImpPostgre.Size = new Size(210, 36);
            btnImpPostgre.TabIndex = 6;
            btnImpPostgre.Text = "Configurar PostgreSQL";
            btnImpPostgre.UseVisualStyleBackColor = false;
            // 
            // btnExpMaria  (Migrar a MariaDB)
            // 
            btnExpMaria.BackColor = Color.FromArgb(50, 60, 80);
            btnExpMaria.FlatAppearance.BorderSize = 0;
            btnExpMaria.FlatStyle = FlatStyle.Flat;
            btnExpMaria.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnExpMaria.ForeColor = Color.FromArgb(163, 190, 140);
            btnExpMaria.Location = new Point(15, 382);
            btnExpMaria.Name = "btnExpMaria";
            btnExpMaria.Size = new Size(210, 36);
            btnExpMaria.TabIndex = 7;
            btnExpMaria.Text = "Migrar a MariaDB";
            btnExpMaria.UseVisualStyleBackColor = false;
            // 
            // btnExpPostgre  (Migrar a PostgreSQL)
            // 
            btnExpPostgre.BackColor = Color.FromArgb(50, 60, 80);
            btnExpPostgre.FlatAppearance.BorderSize = 0;
            btnExpPostgre.FlatStyle = FlatStyle.Flat;
            btnExpPostgre.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnExpPostgre.ForeColor = Color.FromArgb(136, 192, 208);
            btnExpPostgre.Location = new Point(15, 425);
            btnExpPostgre.Name = "btnExpPostgre";
            btnExpPostgre.Size = new Size(210, 36);
            btnExpPostgre.TabIndex = 8;
            btnExpPostgre.Text = "Migrar a PostgreSQL";
            btnExpPostgre.UseVisualStyleBackColor = false;
            // 
            // -- SECCION: ACCIONES --
            // 
            // lblAcciones
            // 
            lblAcciones.AutoSize = true;
            lblAcciones.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblAcciones.ForeColor = Color.FromArgb(156, 163, 175);
            lblAcciones.Location = new Point(15, 480);
            lblAcciones.Name = "lblAcciones";
            lblAcciones.TabIndex = 10;
            lblAcciones.Text = "ACCIONES";
            // 
            // btnLimpiar
            // 
            btnLimpiar.BackColor = Color.FromArgb(64, 66, 73);
            btnLimpiar.FlatAppearance.BorderSize = 0;
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnLimpiar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnLimpiar.ForeColor = Color.FromArgb(209, 213, 219);
            btnLimpiar.Location = new Point(15, 503);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(210, 36);
            btnLimpiar.TabIndex = 11;
            btnLimpiar.Text = "Limpiar Datos";
            btnLimpiar.UseVisualStyleBackColor = false;
            // 

            // 
            // btnConsola
            // 
            btnConsola.BackColor = Color.FromArgb(255, 46, 115);
            btnConsola.FlatAppearance.BorderSize = 0;
            btnConsola.FlatStyle = FlatStyle.Flat;
            btnConsola.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConsola.ForeColor = Color.White;
            btnConsola.Location = new Point(15, 548);
            btnConsola.Name = "btnConsola";
            btnConsola.Size = new Size(210, 40);
            btnConsola.TabIndex = 12;
            btnConsola.Text = "Abrir Consola";
            btnConsola.UseVisualStyleBackColor = false;
            // 
            // btnApi
            // 
            btnApi.BackColor = Color.FromArgb(75, 46, 255);
            btnApi.FlatAppearance.BorderSize = 0;
            btnApi.FlatStyle = FlatStyle.Flat;
            btnApi.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnApi.ForeColor = Color.White;
            btnApi.Location = new Point(15, 600);
            btnApi.Name = "btnApi";
            btnApi.Size = new Size(210, 40);
            btnApi.TabIndex = 13;
            btnApi.Text = "API";
            btnApi.UseVisualStyleBackColor = false;
            // 
            // tabControlPrincipal
            // 
            tabControlPrincipal.Controls.Add(tabTabla);
            tabControlPrincipal.Controls.Add(tabGraficas);
            tabControlPrincipal.Dock = DockStyle.Fill;
            tabControlPrincipal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            tabControlPrincipal.Location = new Point(240, 0);
            tabControlPrincipal.Name = "tabControlPrincipal";
            tabControlPrincipal.SelectedIndex = 0;
            tabControlPrincipal.Size = new Size(960, 700);
            tabControlPrincipal.TabIndex = 2;
            // 
            // tabTabla
            // 
            tabTabla.BackColor = Color.FromArgb(241, 244, 249);
            tabTabla.Controls.Add(dgvDatos);
            tabTabla.Controls.Add(pnlFiltro);
            tabTabla.Location = new Point(4, 32);
            tabTabla.Name = "tabTabla";
            tabTabla.Padding = new Padding(3);
            tabTabla.Size = new Size(952, 664);
            tabTabla.TabIndex = 0;
            tabTabla.Text = "  Tabla de Datos  ";
            // 
            // pnlFiltro
            // 
            pnlFiltro.BackColor = Color.White;
            pnlFiltro.Controls.Add(lblRegistros);
            pnlFiltro.Controls.Add(lblFiltro);
            pnlFiltro.Controls.Add(cmbFiltroColumna);
            pnlFiltro.Controls.Add(btnOrdenar);
            pnlFiltro.Controls.Add(btnAgrupar);
            pnlFiltro.Controls.Add(btnDuplicados);
            pnlFiltro.Dock = DockStyle.Top;
            pnlFiltro.Location = new Point(3, 3);
            pnlFiltro.Name = "pnlFiltro";
            pnlFiltro.Size = new Size(946, 52);
            pnlFiltro.TabIndex = 0;
            // 
            // lblRegistros
            // 
            lblRegistros.AutoSize = true;
            lblRegistros.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblRegistros.ForeColor = Color.FromArgb(37, 99, 235);
            lblRegistros.Location = new Point(15, 13);
            lblRegistros.Name = "lblRegistros";
            lblRegistros.Size = new Size(95, 25);
            lblRegistros.TabIndex = 2;
            lblRegistros.Text = "0 registros";
            // 
            // lblFiltro
            // 
            lblFiltro.AutoSize = true;
            lblFiltro.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFiltro.ForeColor = Color.FromArgb(75, 85, 99);
            lblFiltro.Location = new Point(200, 16);
            lblFiltro.Name = "lblFiltro";
            lblFiltro.Size = new Size(94, 23);
            lblFiltro.TabIndex = 3;
            lblFiltro.Text = "Filtrar por:";
            // 
            // cmbFiltroColumna
            // 
            cmbFiltroColumna.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroColumna.Font = new Font("Segoe UI", 10F);
            cmbFiltroColumna.FormattingEnabled = true;
            cmbFiltroColumna.Location = new Point(295, 12);
            cmbFiltroColumna.Name = "cmbFiltroColumna";
            cmbFiltroColumna.Size = new Size(180, 31);
            cmbFiltroColumna.TabIndex = 4;
            // 
            // btnOrdenar
            // 
            btnOrdenar.BackColor = Color.FromArgb(46, 115, 255);
            btnOrdenar.FlatAppearance.BorderSize = 0;
            btnOrdenar.FlatStyle = FlatStyle.Flat;
            btnOrdenar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOrdenar.ForeColor = Color.White;
            btnOrdenar.Location = new Point(500, 10);
            btnOrdenar.Name = "btnOrdenar";
            btnOrdenar.Size = new Size(130, 32);
            btnOrdenar.TabIndex = 5;
            btnOrdenar.Text = "Ordenar";
            btnOrdenar.UseVisualStyleBackColor = false;
            // 
            // btnAgrupar
            // 
            btnAgrupar.BackColor = Color.FromArgb(46, 115, 255);
            btnAgrupar.FlatAppearance.BorderSize = 0;
            btnAgrupar.FlatStyle = FlatStyle.Flat;
            btnAgrupar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAgrupar.ForeColor = Color.White;
            btnAgrupar.Location = new Point(640, 10);
            btnAgrupar.Name = "btnAgrupar";
            btnAgrupar.Size = new Size(130, 32);
            btnAgrupar.TabIndex = 6;
            btnAgrupar.Text = "Agrupar";
            btnAgrupar.UseVisualStyleBackColor = false;
            // 
            // btnDuplicados
            // 
            btnDuplicados.BackColor = Color.FromArgb(46, 115, 255);
            btnDuplicados.FlatAppearance.BorderSize = 0;
            btnDuplicados.FlatStyle = FlatStyle.Flat;
            btnDuplicados.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDuplicados.ForeColor = Color.White;
            btnDuplicados.Location = new Point(780, 10);
            btnDuplicados.Name = "btnDuplicados";
            btnDuplicados.Size = new Size(140, 32);
            btnDuplicados.TabIndex = 7;
            btnDuplicados.Text = "Duplicados";
            btnDuplicados.UseVisualStyleBackColor = false;
            // 
            // dgvDatos
            // 
            dgvDatos.AllowUserToAddRows = false;
            dgvDatos.AllowUserToDeleteRows = false;
            dgvDatos.BackgroundColor = Color.White;
            dgvDatos.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(37, 99, 235);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvDatos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvDatos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDatos.Dock = DockStyle.Fill;
            dgvDatos.EnableHeadersVisualStyles = false;
            dgvDatos.Font = new Font("Segoe UI", 9.5F);
            dgvDatos.Location = new Point(3, 55);
            dgvDatos.Name = "dgvDatos";
            dgvDatos.ReadOnly = true;
            dgvDatos.RowHeadersWidth = 51;
            dgvDatos.Size = new Size(946, 606);
            dgvDatos.TabIndex = 1;
            // 
            // tabGraficas
            // 
            tabGraficas.BackColor = Color.FromArgb(241, 244, 249);
            tabGraficas.Controls.Add(pnlConfigGrafica);
            tabGraficas.Controls.Add(chartPrincipal);
            tabGraficas.Location = new Point(4, 32);
            tabGraficas.Name = "tabGraficas";
            tabGraficas.Padding = new Padding(3);
            tabGraficas.Size = new Size(952, 664);
            tabGraficas.TabIndex = 1;
            tabGraficas.Text = "  Graficas  ";
            // 
            // pnlConfigGrafica
            // 
            pnlConfigGrafica.BackColor = Color.White;
            pnlConfigGrafica.Controls.Add(lblEjeX);
            pnlConfigGrafica.Controls.Add(cmbEjeX);
            pnlConfigGrafica.Controls.Add(lblEjeY);
            pnlConfigGrafica.Controls.Add(cmbEjeY);
            pnlConfigGrafica.Controls.Add(lblTipo);
            pnlConfigGrafica.Controls.Add(cmbTipoGrafica);
            pnlConfigGrafica.Dock = DockStyle.Top;
            pnlConfigGrafica.Location = new Point(3, 3);
            pnlConfigGrafica.Name = "pnlConfigGrafica";
            pnlConfigGrafica.Size = new Size(946, 62);
            pnlConfigGrafica.TabIndex = 0;
            // 
            // lblEjeX
            // 
            lblEjeX.AutoSize = true;
            lblEjeX.Font = new Font("Segoe UI", 10F);
            lblEjeX.ForeColor = Color.FromArgb(31, 41, 55);
            lblEjeX.Location = new Point(20, 18);
            lblEjeX.Name = "lblEjeX";
            lblEjeX.Size = new Size(54, 23);
            lblEjeX.TabIndex = 0;
            lblEjeX.Text = "Eje X:";
            // 
            // cmbEjeX
            // 
            cmbEjeX.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEjeX.Font = new Font("Segoe UI", 10F);
            cmbEjeX.FormattingEnabled = true;
            cmbEjeX.Location = new Point(80, 14);
            cmbEjeX.Name = "cmbEjeX";
            cmbEjeX.Size = new Size(165, 31);
            cmbEjeX.TabIndex = 1;
            // 
            // lblEjeY
            // 
            lblEjeY.AutoSize = true;
            lblEjeY.Font = new Font("Segoe UI", 10F);
            lblEjeY.ForeColor = Color.FromArgb(31, 41, 55);
            lblEjeY.Location = new Point(262, 18);
            lblEjeY.Name = "lblEjeY";
            lblEjeY.Size = new Size(79, 23);
            lblEjeY.TabIndex = 2;
            lblEjeY.Text = "Eje Y (#):";
            // 
            // cmbEjeY
            // 
            cmbEjeY.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEjeY.Font = new Font("Segoe UI", 10F);
            cmbEjeY.FormattingEnabled = true;
            cmbEjeY.Location = new Point(348, 14);
            cmbEjeY.Name = "cmbEjeY";
            cmbEjeY.Size = new Size(165, 31);
            cmbEjeY.TabIndex = 3;
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Font = new Font("Segoe UI", 10F);
            lblTipo.ForeColor = Color.FromArgb(31, 41, 55);
            lblTipo.Location = new Point(528, 18);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(47, 23);
            lblTipo.TabIndex = 4;
            lblTipo.Text = "Tipo:";
            // 
            // cmbTipoGrafica
            // 
            cmbTipoGrafica.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoGrafica.Font = new Font("Segoe UI", 10F);
            cmbTipoGrafica.FormattingEnabled = true;
            cmbTipoGrafica.Items.AddRange(new object[] { "Barras", "Pastel" });
            cmbTipoGrafica.Location = new Point(581, 14);
            cmbTipoGrafica.Name = "cmbTipoGrafica";
            cmbTipoGrafica.Size = new Size(155, 31);
            cmbTipoGrafica.TabIndex = 5;
            // 

            // 
            // chartPrincipal
            // 
            chartPrincipal.BackColor = Color.White;
            chartPrincipal.Dock = DockStyle.Fill;
            chartPrincipal.Location = new Point(3, 65);
            chartPrincipal.Name = "chartPrincipal";
            chartPrincipal.Size = new Size(946, 596);
            chartPrincipal.TabIndex = 1;
            chartPrincipal.Text = "chartPrincipal";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 700);
            Controls.Add(tabControlPrincipal);
            Controls.Add(pnlImportar);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1024, 640);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Data Arena Fusion";
            pnlImportar.ResumeLayout(false);
            pnlImportar.PerformLayout();
            tabControlPrincipal.ResumeLayout(false);
            tabTabla.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDatos).EndInit();
            pnlFiltro.PerformLayout();
            tabGraficas.ResumeLayout(false);
            pnlConfigGrafica.ResumeLayout(false);
            pnlConfigGrafica.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartPrincipal).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlImportar;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlSeparador;
        private System.Windows.Forms.Label lblImportar;
        private System.Windows.Forms.Label lblExportar;
        private System.Windows.Forms.Label lblAcciones;
        private System.Windows.Forms.Button btnImpCsv;
        private System.Windows.Forms.Button btnImpJson;
        private System.Windows.Forms.Button btnImpXml;
        private System.Windows.Forms.Button btnImpTxt;
        private System.Windows.Forms.Button btnImpMaria;
        private System.Windows.Forms.Button btnImpPostgre;
        private System.Windows.Forms.Button btnExpMaria;
        private System.Windows.Forms.Button btnExpPostgre;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnConsola;
        private System.Windows.Forms.Button btnApi;
        private System.Windows.Forms.TabControl tabControlPrincipal;
        private System.Windows.Forms.TabPage tabTabla;
        private System.Windows.Forms.TabPage tabGraficas;
        private System.Windows.Forms.Panel pnlFiltro;
        private System.Windows.Forms.DataGridView dgvDatos;
        private System.Windows.Forms.Label lblRegistros;
        private System.Windows.Forms.Label lblFiltro;
        private System.Windows.Forms.ComboBox cmbFiltroColumna;
        private System.Windows.Forms.Button btnOrdenar;
        private System.Windows.Forms.Button btnAgrupar;
        private System.Windows.Forms.Button btnDuplicados;
        private System.Windows.Forms.Panel pnlConfigGrafica;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPrincipal;
        private System.Windows.Forms.ComboBox cmbTipoGrafica;
        private System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.ComboBox cmbEjeY;
        private System.Windows.Forms.Label lblEjeY;
        private System.Windows.Forms.ComboBox cmbEjeX;
        private System.Windows.Forms.Label lblEjeX;
    }
}
