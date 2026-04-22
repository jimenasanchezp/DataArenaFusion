using System.Data;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using DataArenaFusion.Forms;
using DataArenaFusion.Core.Models;
using DataArenaFusion.Core.Services;
using DataArenaFusion.Core.Services.Database;

using System.Runtime.InteropServices;
using DataArenaFusion.Core.Processing.Procesadores;

namespace DataArenaFusion
{
    public partial class Form1 : Form
    {
        private readonly GestorDatos _gestorDatos = new();
        private readonly Dictionary<DatabaseProvider, DatabaseConnectionSettings> _conexionesGuardadas = new();
        public Form1()
        {
            InitializeComponent();
            ConectarEventos();
            ConfigurarInterfazInicial();
            ConfigurarGrafica();
            ConfigurarEstiloWeb();
        }

        private void ConectarEventos()
        {
            btnImpCsv.Click += (_, _) => ImportarArchivo("CSV", "Archivos CSV (*.csv)|*.csv", btnImpCsv.Text);
            btnImpJson.Click += (_, _) => ImportarArchivo("JSON", "Archivos JSON (*.json)|*.json", btnImpJson.Text);
            btnImpXml.Click += (_, _) => ImportarArchivo("XML", "Archivos XML (*.xml)|*.xml", btnImpXml.Text);
            btnImpTxt.Click += (_, _) => ImportarArchivo("TXT", "Archivos TXT (*.txt)|*.txt", btnImpTxt.Text);

            btnImpMaria.Click += (_, _) => ConfigurarConexion(DatabaseProvider.MariaDb);
            btnImpPostgre.Click += (_, _) => ConfigurarConexion(DatabaseProvider.PostgreSql);
            btnExpMaria.Click += async (_, _) => await MigrarDatos(DatabaseProvider.MariaDb);
            btnExpPostgre.Click += async (_, _) => await MigrarDatos(DatabaseProvider.PostgreSql);

            btnLimpiar.Click += (_, _) => LimpiarPantalla();
            btnApi.Click += btnApi_Click;

            cmbEjeX.SelectedIndexChanged += (_, _) => GenerarGraficaSiHayDatos();
            cmbEjeY.SelectedIndexChanged += (_, _) => GenerarGraficaSiHayDatos();
            cmbTipoGrafica.SelectedIndexChanged += (_, _) => GenerarGraficaSiHayDatos();

            cmbFiltroColumna.SelectedIndexChanged += (_, _) => AplicarFiltro();

            btnOrdenar.Click += btnOrdenar_Click;
            btnAgrupar.Click += btnAgrupar_Click;
            btnDuplicados.Click += btnDuplicados_Click;
            txtBusqueda.TextChanged += txtBusqueda_TextChanged;
        }

        private void ConfigurarInterfazInicial()
        {
            dgvDatos.AutoGenerateColumns = true;
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgvDatos.ScrollBars = ScrollBars.Both;
            dgvDatos.DataSource = _gestorDatos.TablaActual;

            cmbTipoGrafica.Items.Clear();
            cmbTipoGrafica.Items.AddRange(new object[] { "Barras", "Pastel" });
            cmbTipoGrafica.SelectedIndex = 0;

            ActualizarCombosGrafica(_gestorDatos.TablaActual);
            lblRegistros.Text = "0 registros";
            btnExpMaria.Text = "Migrar a MariaDB";
            btnExpPostgre.Text = "Migrar a PostgreSQL";
        }

        private void ConfigurarEstiloWeb()
        {
            this.Font = new Font("Segoe UI Semibold", 10.5F);
            BackColor = Color.FromArgb(248, 250, 252); // Slate 50
            
            pnlImportar.BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            lblTitulo.ForeColor = Color.FromArgb(236, 72, 153); // Pink 500
            pnlSeparador.BackColor = Color.FromArgb(30, 41, 59); // Slate 800
            
            tabControlPrincipal.BackColor = Color.FromArgb(248, 250, 252);
            tabControlPrincipal.Appearance = TabAppearance.Normal;
            tabControlPrincipal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            dgvDatos.BackgroundColor = Color.White;
            dgvDatos.GridColor = Color.FromArgb(241, 245, 249);
            dgvDatos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvDatos.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvDatos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvDatos.ColumnHeadersHeight = 50;
            
            dgvDatos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvDatos.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
            dgvDatos.RowTemplate.Height = 45;

            foreach (Control ctrl in pnlImportar.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.Cursor = Cursors.Hand;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Font = new Font("Segoe UI Semibold", 10F);
                    // Los colores individuales se mantienen del Designer pero los suavizamos aquí si es necesario
                }
                else if (ctrl is Label lbl)
                {
                    lbl.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                }
            }
            
            btnApi.BackColor = Color.FromArgb(139, 92, 246); // Violet 500
            btnApi.ForeColor = Color.White;
            btnApi.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        private void btnOrdenar_Click(object sender, EventArgs e)
        {
            if (_gestorDatos.RegistrosActuales.Count == 0 || cmbFiltroColumna.SelectedItem == null)
            {
                MessageBox.Show("Carga un archivo y selecciona una columna para ordenar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string columna = cmbFiltroColumna.SelectedItem.ToString();
            if (columna == "Todas las columnas")
            {
                MessageBox.Show("Selecciona una columna específica para ordenar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            Cursor = Cursors.WaitCursor;
            
            // Pausar UI
            dgvDatos.SuspendLayout();
            var modoAnterior = dgvDatos.AutoSizeColumnsMode;
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // Desactiva recálculo de ancho por fila

            var procesador = new ProcesadorOrdenamiento(_gestorDatos.ColId, _gestorDatos.ColCat, _gestorDatos.ColVal);
            procesador.Procesar(_gestorDatos.RegistrosActuales, columna);
            
            _gestorDatos.SincronizarTablaDesdeMemoria();
            dgvDatos.DataSource = null;
            dgvDatos.DataSource = _gestorDatos.TablaActual;
            
            // Restaurar UI
            dgvDatos.AutoSizeColumnsMode = modoAnterior;
            dgvDatos.ResumeLayout();
            
            Cursor = Cursors.Default;
            
            MessageBox.Show($"Datos ordenados por '{columna}' exitosamente usando QuickSort.", "Ordenamiento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnAgrupar_Click(object sender, EventArgs e)
        {
            if (_gestorDatos.RegistrosActuales.Count == 0 || cmbFiltroColumna.SelectedItem == null)
            {
                MessageBox.Show("Carga un archivo y selecciona una columna en el Buscador para agrupar o contar sus registros.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string columna = cmbFiltroColumna.SelectedItem.ToString();
            if (columna == "Todas las columnas")
            {
                MessageBox.Show("Selecciona una columna específica para agrupar (ej: Ciudad o Categoría).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var procesador = new ProcesadorAgrupamiento(_gestorDatos.ColId, _gestorDatos.ColCat, _gestorDatos.ColVal);
            var agrupados = procesador.Procesar(_gestorDatos.RegistrosActuales, columna);
            
            if (agrupados.Count == 0)
            {
                MessageBox.Show("No hay datos suficientes para agrupar por esta columna.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var formReporte = new Form
            {
                Text = $"Resumen por {columna} - Data Arena Fusion",
                Size = new Size(500, 550),
                StartPosition = FormStartPosition.CenterParent,
                Icon = this.Icon
            };
            
            var dgvReporte = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            
            dgvReporte.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvReporte.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReporte.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvReporte.ColumnHeadersHeight = 40;

            dgvReporte.Columns.Add("Key", columna);
            
            string tituloValor = string.IsNullOrEmpty(_gestorDatos.ColVal) ? "Conteo de Registros" : $"Total ({_gestorDatos.ColVal})";
            dgvReporte.Columns.Add("Sum", tituloValor);

            bool isCounting = string.IsNullOrEmpty(_gestorDatos.ColVal);
            foreach(var item in agrupados)
            {
                dgvReporte.Rows.Add(item.Key, isCounting ? item.Value.ToString("N0") : item.Value.ToString("N2"));
            }
            
            formReporte.Controls.Add(dgvReporte);
            formReporte.ShowDialog();
        }

        private void btnDuplicados_Click(object sender, EventArgs e)
        {
            if (_gestorDatos.RegistrosActuales.Count == 0 || cmbFiltroColumna.SelectedItem == null)
            {
                MessageBox.Show("Carga un archivo y selecciona una columna en el Buscador para detectar valores repetidos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string columna = cmbFiltroColumna.SelectedItem.ToString();
            if (columna == "Todas las columnas")
            {
                MessageBox.Show("Selecciona una columna específica para buscar duplicados.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var procesador = new ProcesadorDuplicados(_gestorDatos.ColId, _gestorDatos.ColCat, _gestorDatos.ColVal);
            var duplicados = procesador.Procesar(_gestorDatos.RegistrosActuales, columna);
            
            if (duplicados.Count == 0)
            {
                MessageBox.Show($"No se encontraron duplicados en la columna '{columna}'.", "Duplicados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;
            dgvDatos.SuspendLayout();

            // 1. Construir un índice ultra rápido (O(1)) de los valores repetidos
            var valoresDuplicados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in duplicados)
            {
                string val = "";
                if (columna == _gestorDatos.ColId) val = d.Id.ToString();
                else if (columna == _gestorDatos.ColCat) val = d.Categoria;
                else if (columna == _gestorDatos.ColVal) val = d.Valor.ToString(CultureInfo.InvariantCulture);
                else { d.Extras.TryGetValue(columna, out val); val ??= ""; }
                
                valoresDuplicados.Add(val);
            }

            // 2. Limpiar y Resaltar en un solo ciclo rápido
            int resaltados = 0;
            if (dgvDatos.Columns.Contains(columna))
            {
                foreach (DataGridViewRow row in dgvDatos.Rows)
                {
                    string val = row.Cells[columna].Value?.ToString() ?? "";

                    if (valoresDuplicados.Contains(val))
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        resaltados++;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                    }
                }
            }

            dgvDatos.ResumeLayout();
            Cursor = Cursors.Default;

            MessageBox.Show($"Se detectaron y resaltaron {resaltados} filas con datos duplicados en '{columna}'.", "Detección de Duplicados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            string termino = txtBusqueda.Text;
            
            // Si no hay termino, mostramos todo
            if (string.IsNullOrWhiteSpace(termino))
            {
                dgvDatos.DataSource = _gestorDatos.TablaActual;
                lblRegistros.Text = $"{_gestorDatos.TablaActual.Rows.Count} registros";
                return;
            }

            // Usar la lógica de filtrado del Core (Nivel 5)
            var filtrados = _gestorDatos.Filtrar(termino);
            
            // Crear una vista temporal para el DataGridView
            // Nota: En una app real de alto rendimiento usaríamos un DataView con filtros
            // pero para los requisitos de la clase, demostrar el filtrado sobre la lista común es ideal.
            DataTable tablaFiltrada = _gestorDatos.TablaActual.Clone();
            foreach (var reg in filtrados)
            {
                var fila = tablaFiltrada.NewRow();
                fila[_gestorDatos.ColId] = reg.Id.ToString();
                fila[_gestorDatos.ColCat] = reg.Categoria;
                fila[_gestorDatos.ColVal] = reg.Valor.ToString(CultureInfo.InvariantCulture);
                foreach (var extra in reg.Extras)
                {
                    if (tablaFiltrada.Columns.Contains(extra.Key)) fila[extra.Key] = extra.Value;
                }
                tablaFiltrada.Rows.Add(fila);
            }

            dgvDatos.DataSource = tablaFiltrada;
            lblRegistros.Text = $"{filtrados.Count} resultados";
        }



        private async void btnApi_Click(object sender, EventArgs e)
        {
            if (_gestorDatos.TablaActual.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos cargados para enriquecer.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnApi.Enabled = false;
            btnApi.Text = "Procesando...";
            Cursor = Cursors.WaitCursor;

            // PREVENCION DE ERROR: Desconectar la tabla de la interfaz (UI thread)
            // ANTES de modificarla en segundo plano (Background Thread) para evitar NullReferenceException
            dgvDatos.SuspendLayout();
            dgvDatos.DataSource = null;
            dgvDatos.ResumeLayout();

            try
            {
                // Enriquecer de forma asincrona para no bloquear UI
                await DataEnricherService.EnriquecerDataTableAsync(_gestorDatos.TablaActual);

                // Volver a reconectar la tabla a la Interfaz de forma segura
                dgvDatos.SuspendLayout();
                dgvDatos.DataSource = _gestorDatos.TablaActual;
                
                // Formatear columnas financieras
                foreach (DataGridViewColumn col in dgvDatos.Columns)
                {
                    if (col.Name.EndsWith("(MXN)"))
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
                
                dgvDatos.ResumeLayout();
                
                MessageBox.Show("Datos enriquecidos con éxito desde las APIs de internet.", "API", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la API: {ex.Message}", "Error API", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnApi.Enabled = true;
                btnApi.Text = "API";
                Cursor = Cursors.Default;
            }
        }


        private void ConfigurarGrafica()
        {
            if (chartPrincipal == null)
            {
                chartPrincipal = new Chart();
            }

            if (!tabGraficas.Controls.Contains(chartPrincipal))
            {
                tabGraficas.Controls.Add(chartPrincipal);
            }

            chartPrincipal.Dock = DockStyle.Fill;
            chartPrincipal.BackColor = Color.White;
            chartPrincipal.BackGradientStyle = GradientStyle.TopBottom;
            chartPrincipal.BackSecondaryColor = Color.White;
            chartPrincipal.Palette = ChartColorPalette.BrightPastel;
            chartPrincipal.BorderlineDashStyle = ChartDashStyle.Solid;
            chartPrincipal.BorderlineColor = Color.FromArgb(229, 231, 235);
            chartPrincipal.BorderlineWidth = 1;
            chartPrincipal.AntiAliasing = AntiAliasingStyles.All;
            chartPrincipal.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            chartPrincipal.ChartAreas.Clear();
            var area = new ChartArea("Principal")
            {
                BackColor = Color.White
            };
            area.BackGradientStyle = GradientStyle.TopBottom;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(229, 231, 235);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(229, 231, 235);
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Solid;
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            area.AxisX.LabelStyle.Angle = -35;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8.5F);
            area.AxisX.LabelStyle.ForeColor = Color.FromArgb(75, 85, 99);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8.5F);
            area.AxisY.LabelStyle.ForeColor = Color.FromArgb(75, 85, 99);
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisX.LineColor = Color.FromArgb(203, 213, 225);
            area.AxisY.LineColor = Color.FromArgb(203, 213, 225);
            chartPrincipal.ChartAreas.Add(area);

            chartPrincipal.Legends.Clear();
            chartPrincipal.Legends.Add(new Legend("Leyenda")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(75, 85, 99),
                BackColor = Color.Transparent
            });

            chartPrincipal.Titles.Clear();
            chartPrincipal.Titles.Add(new Title
            {
                Text = "Importa datos para generar una grafica",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Alignment = ContentAlignment.TopLeft
            });
            chartPrincipal.Series.Clear();
        }

        private async void ImportarArchivo(string formato, string filtro, string origen)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = $"Importar {formato}",
                Filter = filtro,
                CheckFileExists = true,
                CheckPathExists = true
            };

            var carpetaEjemplos = Path.Combine(AppContext.BaseDirectory, "TestData");
            if (Directory.Exists(carpetaEjemplos))
            {
                dialogo.InitialDirectory = carpetaEjemplos;
            }

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                dgvDatos.DataSource = null; // Unbind prevent cross-thread issues

                await Task.Run(() => _gestorDatos.CargarArchivo(dialogo.FileName));
                
                RefrescarVista();

                MessageBox.Show(
                    $"Se cargaron {_gestorDatos.TablaActual.Rows.Count} registros y {_gestorDatos.TablaActual.Columns.Count} columnas desde {Path.GetFileName(dialogo.FileName)}.",
                    $"Importación {formato}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                RefrescarVista();
                MessageBox.Show(
                    $"No se pudo importar el archivo seleccionado.\n\nOrigen: {origen}\nFormato: {formato}\n\nDetalle: {ex.Message}",
                    $"Error al importar {formato}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefrescarVista()
        {
            dgvDatos.DataSource = null;
            dgvDatos.DataSource = _gestorDatos.TablaActual;

            lblRegistros.Text = $"{_gestorDatos.TablaActual.Rows.Count} registros";
            ActualizarCombosGrafica(_gestorDatos.TablaActual);
            RefrescarFiltros();
            AutoConfigurarGrafica();
            GenerarGrafica();
        }

        private void RefrescarFiltros()
        {
            cmbFiltroColumna.SelectedIndexChanged -= Filtro_SelectedIndexChanged;

            var columnas = _gestorDatos.TablaActual.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName).ToArray();
                
            cmbFiltroColumna.Items.Clear();
            cmbFiltroColumna.Items.Add("Todas las columnas");
            if (columnas.Length > 0) cmbFiltroColumna.Items.AddRange(columnas);
            
            if (cmbFiltroColumna.Items.Count > 0) cmbFiltroColumna.SelectedIndex = 0;

            cmbFiltroColumna.SelectedIndexChanged += Filtro_SelectedIndexChanged;
        }

        private void Filtro_SelectedIndexChanged(object sender, EventArgs e) => AplicarFiltro();

        private void AplicarFiltro()
        {
            if (cmbFiltroColumna.SelectedItem == null || dgvDatos.Columns.Count == 0) return;
            string columna = cmbFiltroColumna.SelectedItem.ToString();
            
            if (columna == "Todas las columnas")
            {
                foreach (DataGridViewColumn col in dgvDatos.Columns)
                {
                    col.Visible = true;
                }
            }
            else
            {
                foreach (DataGridViewColumn col in dgvDatos.Columns)
                {
                    col.Visible = (col.Name == columna);
                }
            }
        }



        private void ActualizarCombosGrafica(DataTable tabla)
        {
            var columnas = tabla.Columns.Cast<DataColumn>()
                .Select(col => col.ColumnName)
                .Where(col => !string.Equals(col, "Fuente", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            cmbEjeX.BeginUpdate();
            cmbEjeY.BeginUpdate();
            try
            {
                cmbEjeX.Items.Clear();
                cmbEjeX.Items.AddRange(columnas);

                cmbEjeY.Items.Clear();
                cmbEjeY.Items.AddRange(columnas);
            }
            finally
            {
                cmbEjeX.EndUpdate();
                cmbEjeY.EndUpdate();
            }

            if (cmbEjeX.Items.Count > 0)
            {
                cmbEjeX.SelectedIndex = 0;
            }
            else
            {
                cmbEjeX.SelectedIndex = -1;
            }

            if (cmbEjeY.Items.Count > 1)
            {
                cmbEjeY.SelectedIndex = 1;
            }
            else if (cmbEjeY.Items.Count == 1)
            {
                cmbEjeY.SelectedIndex = 0;
            }
            else
            {
                cmbEjeY.SelectedIndex = -1;
            }

            if (cmbTipoGrafica.Items.Count > 0 && cmbTipoGrafica.SelectedIndex < 0)
            {
                cmbTipoGrafica.SelectedIndex = 0;
            }
        }

        private void AutoConfigurarGrafica()
        {
            var tabla = _gestorDatos.TablaActual;
            if (tabla.Columns.Count == 0)
            {
                return;
            }

            var columnas = tabla.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .Where(c => !string.Equals(c, "Fuente", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var columnasNumericas = columnas.Where(col => EsColumnaNumerica(tabla, col)).ToList();
            var columnasTexto = columnas.Where(col => !EsColumnaNumerica(tabla, col)).ToList();

            var ejeX = columnasTexto.FirstOrDefault() ?? columnas.FirstOrDefault();
            var ejeY = columnasNumericas.FirstOrDefault();

            if (ejeY == null && columnas.Count > 1)
            {
                ejeY = columnas.Skip(columnas.First() == ejeX ? 1 : 0).FirstOrDefault(col => col != ejeX);
            }

            if (!string.IsNullOrWhiteSpace(ejeX) && cmbEjeX.Items.Contains(ejeX))
            {
                cmbEjeX.SelectedItem = ejeX;
            }

            if (!string.IsNullOrWhiteSpace(ejeY) && cmbEjeY.Items.Contains(ejeY))
            {
                cmbEjeY.SelectedItem = ejeY;
            }

            if (cmbTipoGrafica.SelectedIndex < 0)
            {
                cmbTipoGrafica.SelectedIndex = 0;
            }
        }

        private void GenerarGraficaSiHayDatos()
        {
            if (_gestorDatos.TablaActual.Rows.Count == 0)
            {
                return;
            }

            GenerarGrafica();
        }

        private void GenerarGrafica()
        {
            if (chartPrincipal == null)
            {
                return;
            }

            var tabla = ObtenerTablaGraficaActual();
            if (tabla.Rows.Count == 0 || tabla.Columns.Count == 0)
            {
                MostrarGraficaVacia("Importa datos para generar una grafica.");
                return;
            }

            var ejeX = cmbEjeX.SelectedItem?.ToString();
            var ejeY = cmbEjeY.SelectedItem?.ToString();
            var tipo = cmbTipoGrafica.SelectedItem?.ToString() ?? "Barras";

            if (string.IsNullOrWhiteSpace(ejeX) || !tabla.Columns.Contains(ejeX))
            {
                ejeX = tabla.Columns[0].ColumnName;
            }

            if (string.IsNullOrWhiteSpace(ejeY) || !tabla.Columns.Contains(ejeY))
            {
                ejeY = tabla.Columns.Count > 1 ? tabla.Columns[1].ColumnName : ejeX;
            }

            chartPrincipal.Series.Clear();
            chartPrincipal.Titles.Clear();
            chartPrincipal.Titles.Add(new Title($"{tipo}: {ejeX} vs {ejeY}") { ForeColor = Color.FromArgb(31, 41, 55) });

            if (string.Equals(tipo, "Pastel", StringComparison.OrdinalIgnoreCase))
            {
                GenerarPastel(tabla, ejeX!, ejeY!);
                return;
            }

            GenerarBarras(tabla, ejeX!, ejeY!);
        }

        private void GenerarBarras(DataTable tabla, string ejeX, string ejeY)
        {
            var serie = new Series("Datos")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = "Principal",
                Legend = "Leyenda",
                IsValueShownAsLabel = true,
                LabelFormat = "N0"
            };

            var usoSuma = EsColumnaNumerica(tabla, ejeY);
            var puntos = tabla.AsEnumerable()
                .GroupBy(fila => fila[ejeX]?.ToString() ?? string.Empty)
                .Select(grupo => new
                {
                    Label = string.IsNullOrWhiteSpace(grupo.Key) ? "(Vacio)" : grupo.Key,
                    Value = usoSuma
                        ? grupo.Sum(fila => ObtenerNumero(fila[ejeY]?.ToString()))
                        : grupo.Count()
                })
                .OrderByDescending(item => item.Value)
                .Take(20)
                .ToList();

            foreach (var punto in puntos)
            {
                serie.Points.AddXY(punto.Label, punto.Value);
            }

            serie["PointWidth"] = "0.65";
            serie["BarLabelStyle"] = "Center";
            serie.Color = Color.FromArgb(37, 99, 235);

            chartPrincipal.Series.Add(serie);
            AjustarEjesParaSeries();
        }

        private void GenerarPastel(DataTable tabla, string ejeX, string ejeY)
        {
            var serie = new Series("Datos")
            {
                ChartType = SeriesChartType.Pie,
                ChartArea = "Principal",
                Legend = "Leyenda",
                IsValueShownAsLabel = true
            };

            var usoSuma = EsColumnaNumerica(tabla, ejeY);
            var grupos = tabla.AsEnumerable()
                .GroupBy(fila => fila[ejeX]?.ToString() ?? string.Empty)
                .Select(grupo => new
                {
                    Label = string.IsNullOrWhiteSpace(grupo.Key) ? "(Vacio)" : grupo.Key,
                    Value = usoSuma
                        ? grupo.Sum(fila => ObtenerNumero(fila[ejeY]?.ToString()))
                        : grupo.Count()
                })
                .OrderByDescending(item => item.Value)
                .Take(10)
                .ToList();

            foreach (var grupo in grupos)
            {
                serie.Points.AddXY(grupo.Label, grupo.Value);
            }

            serie.Color = Color.FromArgb(99, 102, 241);
            serie["PieLabelStyle"] = "Outside";
            serie["PieDrawingStyle"] = "SoftEdge";
            serie["CollectedThreshold"] = "5";
            serie["CollectedLabel"] = "Otros";
            serie.BorderColor = Color.White;
            serie.BorderWidth = 2;

            chartPrincipal.Series.Add(serie);
            AjustarEjesParaSeries();
        }

        private DataTable ObtenerTablaGraficaActual()
        {
            if (dgvDatos.DataSource is DataView vista)
            {
                return vista.ToTable();
            }

            if (dgvDatos.DataSource is DataTable tabla)
            {
                return tabla;
            }

            return _gestorDatos.TablaActual;
        }

        private void AjustarEjesParaSeries()
        {
            var area = chartPrincipal.ChartAreas["Principal"];
            area.AxisX.TitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            area.AxisX.TitleForeColor = Color.FromArgb(31, 41, 55);
            area.AxisY.TitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            area.AxisY.TitleForeColor = Color.FromArgb(31, 41, 55);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9F);
            area.AxisX.LabelStyle.ForeColor = Color.FromArgb(75, 85, 99);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9F);
            area.AxisY.LabelStyle.ForeColor = Color.FromArgb(75, 85, 99);
            area.AxisX.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.Enabled = true;
            area.RecalculateAxesScale();
        }

        private void MostrarGraficaVacia(string mensaje)
        {
            chartPrincipal.Series.Clear();
            chartPrincipal.Titles.Clear();
            chartPrincipal.Titles.Add(new Title
            {
                Text = mensaje,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Alignment = ContentAlignment.MiddleCenter
            });
        }

        private static bool EsColumnaNumerica(DataTable tabla, string nombreColumna)
        {
            var evaluadas = 0;
            var exitosas = 0;

            foreach (DataRow fila in tabla.Rows)
            {
                if (evaluadas >= 20)
                {
                    break;
                }

                var texto = fila[nombreColumna]?.ToString();
                if (string.IsNullOrWhiteSpace(texto))
                {
                    continue;
                }

                evaluadas++;
                if (double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out _) ||
                    double.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out _))
                {
                    exitosas++;
                }
            }

            return evaluadas > 0 && exitosas == evaluadas;
        }

        private static double ObtenerNumero(string? texto)
        {
            if (double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
            {
                return valor;
            }

            if (double.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out valor))
            {
                return valor;
            }

            return 0;
        }

        private void LimpiarPantalla()
        {
            _gestorDatos.Limpiar();
            dgvDatos.DataSource = null;
            dgvDatos.DataSource = _gestorDatos.TablaActual;

            cmbEjeX.Items.Clear();
            cmbEjeY.Items.Clear();
            cmbTipoGrafica.Items.Clear();
            cmbTipoGrafica.Items.AddRange(new object[] { "Barras", "Pastel" });
            cmbTipoGrafica.SelectedIndex = 0;
            lblRegistros.Text = "0 registros";
            MostrarGraficaVacia("Importa datos para generar una grafica.");
        }



        private void ConfigurarConexion(DatabaseProvider provider)
        {
            var settings = ObtenerConfiguracionGuardada(provider);
            using var dialogo = new DatabaseConnectionForm(provider, settings);

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _conexionesGuardadas[provider] = dialogo.Settings;

            var servicio = DatabaseConnectionServiceFactory.Create(provider);
            var resumen = $"Host: {dialogo.Settings.Host}\nPuerto: {dialogo.Settings.Port}\nBase: {dialogo.Settings.Database}\nUsuario: {dialogo.Settings.Username}";

            MessageBox.Show(
                $"Conexion preparada para {servicio.DisplayName}.\n\n{resumen}",
                "Conexion guardada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async Task MigrarDatos(DatabaseProvider provider)
        {
            if (_gestorDatos.TablaActual.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Primero importa uno o mas archivos antes de migrar los datos.",
                    "Sin datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var settings = ObtenerConfiguracionGuardada(provider);
            using var dialogo = new DatabaseConnectionForm(provider, settings);

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _conexionesGuardadas[provider] = dialogo.Settings;

            try
            {
                Cursor = Cursors.WaitCursor;
                var servicio = DatabaseConnectionServiceFactory.Create(provider);
                var resumen = await Task.Run(() => servicio.MigrarDatos(dialogo.Settings, _gestorDatos.TablaActual));

                MessageBox.Show(
                    resumen,
                    $"Migracion {servicio.DisplayName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo completar la migracion.\n\nDetalle: {ex.Message}",
                    "Error de migracion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AbrirConexion(DatabaseProvider provider)
        {
            var settings = ObtenerConfiguracionGuardada(provider);
            using var dialogo = new DatabaseConnectionForm(provider, settings);

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _conexionesGuardadas[provider] = dialogo.Settings;

            var servicio = DatabaseConnectionServiceFactory.Create(provider);
            var resumen = $"Host: {dialogo.Settings.Host}\nPuerto: {dialogo.Settings.Port}\nBase: {dialogo.Settings.Database}\nUsuario: {dialogo.Settings.Username}";

            MessageBox.Show(
                $"Conexion preparada para {servicio.DisplayName}.\n\n{resumen}",
                "Conexion guardada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private DatabaseConnectionSettings? ObtenerConfiguracionGuardada(DatabaseProvider provider)
        {
            return _conexionesGuardadas.TryGetValue(provider, out var settings)
                ? settings.Clone()
                : null;
        }

        private static void MostrarNoDisponible(string funcionalidad)
        {
            MessageBox.Show(
                $"{funcionalidad} todavía no está conectada.",
                "Funcionalidad pendiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
