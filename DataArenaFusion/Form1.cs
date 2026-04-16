using System.Data;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using DataArenaFusion.Forms;
using DataArenaFusion.Models;
using DataArenaFusion.Services;
using DataArenaFusion.Services.Database;

using System.Runtime.InteropServices;
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

            //btnGraficar.Click += (_, _) => GenerarGrafica();
            //btnGenerarGrafica.Click += (_, _) => GenerarGrafica();
            btnLimpiar.Click += (_, _) => LimpiarPantalla();
            btnConsola.Click += btnConsola_Click;

            cmbEjeX.SelectedIndexChanged += (_, _) => GenerarGraficaSiHayDatos();
            cmbEjeY.SelectedIndexChanged += (_, _) => GenerarGraficaSiHayDatos();
            cmbTipoGrafica.SelectedIndexChanged += (_, _) => GenerarGraficaSiHayDatos();

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
            this.Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(241, 244, 249);
            tabControlPrincipal.BackColor = Color.FromArgb(241, 244, 249);
            tabControlPrincipal.Appearance = TabAppearance.Normal;
            tabControlPrincipal.DrawMode = TabDrawMode.Normal;
            tabControlPrincipal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            dgvDatos.BackgroundColor = Color.White;
            dgvDatos.BorderStyle = BorderStyle.None;
            dgvDatos.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvDatos.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvDatos.EnableHeadersVisualStyles = false;
            
            dgvDatos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvDatos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDatos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            dgvDatos.ColumnHeadersHeight = 45;
            
            dgvDatos.DefaultCellStyle.BackColor = Color.White;
            dgvDatos.DefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
            dgvDatos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvDatos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvDatos.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
            
            dgvDatos.GridColor = Color.FromArgb(229, 231, 235);
            dgvDatos.RowHeadersVisible = false;
            dgvDatos.RowTemplate.Height = 45;
            dgvDatos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            pnlFiltro.BackColor = Color.White;
            pnlConfigGrafica.BackColor = Color.White;
            tabTabla.BackColor = Color.FromArgb(241, 244, 249);
            tabGraficas.BackColor = Color.FromArgb(241, 244, 249);

            foreach (Control ctrl in pnlImportar.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.Cursor = Cursors.Hand;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Size = new Size(210, 36);
                    btn.Location = new Point((pnlImportar.Width - btn.Width) / 2, btn.Location.Y);
                    
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                        Math.Min(255, btn.BackColor.R + 30),
                        Math.Min(255, btn.BackColor.G + 30),
                        Math.Min(255, btn.BackColor.B + 30)
                    );
                }
            }

                //btnGraficar.Cursor = Cursors.Hand;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        private void btnConsola_Click(object sender, EventArgs e)
        {
            if (_gestorDatos.TablaActual == null || _gestorDatos.TablaActual.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos cargados para mostrar en la consola.", "Consola", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Abrir la terminal negra tipo CMD
            AllocConsole();

            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("       DATA ARENA FUSION - CONSOLA DE DATOS       ");
            Console.WriteLine("==================================================");
            Console.WriteLine($"Total Registros: {_gestorDatos.TablaActual.Rows.Count}");
            Console.WriteLine();

            // Imprimir Encabezados
            var encabezados = _gestorDatos.TablaActual.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            Console.WriteLine(string.Join(" | ", encabezados.Select(e => e.PadRight(15))));
            Console.WriteLine(new string('-', 15 * encabezados.Count + (3 * encabezados.Count)));

            // Imprimir Filas (hasta un maximo para no saturar)
            int limite = Math.Min(100, _gestorDatos.TablaActual.Rows.Count);
            for (int i = 0; i < limite; i++)
            {
                var fila = _gestorDatos.TablaActual.Rows[i];
                var valores = encabezados.Select(c => (fila[c]?.ToString() ?? "").PadRight(15).Substring(0, Math.Min((fila[c]?.ToString() ?? "").Length, 15)+Math.Max(0, 15 - (fila[c]?.ToString() ?? "").Length)));
                Console.WriteLine(string.Join(" | ", valores));
            }

            if (_gestorDatos.TablaActual.Rows.Count > limite)
            {
                Console.WriteLine($"\n... y {_gestorDatos.TablaActual.Rows.Count - limite} registros más (ocultos).");
            }

            var serie = chartPrincipal.Series.FirstOrDefault();
            if (serie != null && serie.Points.Count > 0)
            {
                Console.WriteLine("\n==================================================");
                Console.WriteLine("                  GRAFICA ASCII                   ");
                Console.WriteLine("==================================================");
                Console.WriteLine($"Eje X: {cmbEjeX.SelectedItem} | Eje Y: {cmbEjeY.SelectedItem}\n");

                double maxY = serie.Points.Max(p => p.YValues[0]);
                int maxBarWidth = 30; // caracteres
                
                foreach(var point in serie.Points)
                {
                    string label = string.IsNullOrWhiteSpace(point.AxisLabel) ? "X" : point.AxisLabel;
                    if (label.Length > 12) label = label.Substring(0, 9) + "...";
                    
                    int barLength = maxY > 0 ? (int)((point.YValues[0] / maxY) * maxBarWidth) : 0;
                    string bar = new string('#', barLength);
                    Console.WriteLine($"{label.PadRight(12)} | {bar} {point.YValues[0]}");
                }
            }

            Console.WriteLine("\n[Datos mostrados. Esta consola refleja el estado actual en memoria.]");
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

            var carpetaEjemplos = Path.Combine(AppContext.BaseDirectory, "SampleData");
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
            AutoConfigurarGrafica();
            GenerarGrafica();
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
