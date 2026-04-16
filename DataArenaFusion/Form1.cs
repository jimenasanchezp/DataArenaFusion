using System.Data;
using DataArenaFusion.Services;

namespace DataArenaFusion
{
    public partial class Form1 : Form
    {
        private readonly GestorDatos _gestorDatos = new();
        private bool _actualizandoFiltroFuente;

        public Form1()
        {
            InitializeComponent();
            ConectarEventos();
            ConfigurarInterfazInicial();
        }

        private void ConectarEventos()
        {
            btnImpCsv.Click += (_, _) => ImportarArchivo("CSV", "Archivos CSV (*.csv)|*.csv", btnImpCsv.Text);
            btnImpJson.Click += (_, _) => ImportarArchivo("JSON", "Archivos JSON (*.json)|*.json", btnImpJson.Text);
            btnImpXml.Click += (_, _) => ImportarArchivo("XML", "Archivos XML (*.xml)|*.xml", btnImpXml.Text);
            btnImpTxt.Click += (_, _) => ImportarArchivo("TXT", "Archivos TXT (*.txt)|*.txt", btnImpTxt.Text);

            btnImpSql.Click += (_, _) => MostrarNoDisponible("Importación desde SQL Server");
            btnImpMaria.Click += (_, _) => MostrarNoDisponible("Importación desde MariaDB");
            btnImpPostgre.Click += (_, _) => MostrarNoDisponible("Importación desde PostgreSQL");

            cmbFiltroFuente.SelectedIndexChanged += OnFiltroFuenteChanged;
        }

        private void ConfigurarInterfazInicial()
        {
            dgvDatos.AutoGenerateColumns = true;
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDatos.DataSource = _gestorDatos.TablaActual;

            cmbFiltroFuente.Items.Clear();
            cmbFiltroFuente.Items.Add("Todos");
            cmbFiltroFuente.SelectedIndex = 0;

            ActualizarCombosGrafica(_gestorDatos.TablaActual);
            lblRegistros.Text = "0 registros";
        }

        private void ImportarArchivo(string formato, string filtro, string origen)
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
                _gestorDatos.CargarArchivo(dialogo.FileName);
                RefrescarVista();

                MessageBox.Show(
                    $"Se cargaron {_gestorDatos.TablaActual.Rows.Count} registros y {_gestorDatos.TablaActual.Columns.Count} columnas desde {Path.GetFileName(dialogo.FileName)}.",
                    $"Importación {formato}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo importar el archivo seleccionado.\n\nOrigen: {origen}\nFormato: {formato}\n\nDetalle: {ex.Message}",
                    $"Error al importar {formato}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RefrescarVista()
        {
            dgvDatos.DataSource = null;
            dgvDatos.DataSource = _gestorDatos.TablaActual;

            lblRegistros.Text = $"{_gestorDatos.TablaActual.Rows.Count} registros";
            ActualizarFiltroFuente(_gestorDatos.TablaActual);
            ActualizarCombosGrafica(_gestorDatos.TablaActual);
        }

        private void ActualizarFiltroFuente(DataTable tabla)
        {
            var fuenteActual = cmbFiltroFuente.SelectedItem?.ToString() ?? "Todos";
            _actualizandoFiltroFuente = true;

            try
            {
                cmbFiltroFuente.Items.Clear();
                cmbFiltroFuente.Items.Add("Todos");

                if (tabla.Columns.Contains("Categoria"))
                {
                    var categorias = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (DataRow fila in tabla.Rows)
                    {
                        var categoria = fila["Categoria"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(categoria))
                        {
                            categorias.Add(categoria);
                        }
                    }

                    foreach (var categoria in categorias.OrderBy(x => x))
                    {
                        cmbFiltroFuente.Items.Add(categoria);
                    }

                    cmbFiltroFuente.Enabled = true;
                    cmbFiltroFuente.SelectedItem = cmbFiltroFuente.Items.Contains(fuenteActual)
                        ? fuenteActual
                        : "Todos";
                }
                else
                {
                    cmbFiltroFuente.Enabled = false;
                    cmbFiltroFuente.SelectedIndex = 0;
                }

                if (cmbFiltroFuente.SelectedIndex < 0 && cmbFiltroFuente.Items.Count > 0)
                {
                    cmbFiltroFuente.SelectedIndex = 0;
                }
            }
            finally
            {
                _actualizandoFiltroFuente = false;
            }
        }

        private void OnFiltroFuenteChanged(object? sender, EventArgs e)
        {
            if (_actualizandoFiltroFuente)
            {
                return;
            }

            AplicarFiltroFuente();
        }

        private void AplicarFiltroFuente()
        {
            if (!_gestorDatos.TablaActual.Columns.Contains("Categoria"))
            {
                dgvDatos.DataSource = _gestorDatos.TablaActual;
                lblRegistros.Text = $"{_gestorDatos.TablaActual.Rows.Count} registros";
                return;
            }

            var filtro = cmbFiltroFuente.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(filtro) || filtro == "Todos")
            {
                dgvDatos.DataSource = _gestorDatos.TablaActual;
                lblRegistros.Text = $"{_gestorDatos.TablaActual.Rows.Count} registros";
                return;
            }

            var vista = _gestorDatos.TablaActual.DefaultView;
            vista.RowFilter = $"[Categoria] = '{EscaparParaRowFilter(filtro)}'";
            dgvDatos.DataSource = vista.ToTable();
            lblRegistros.Text = $"{vista.Count} registros";
        }

        private void ActualizarCombosGrafica(DataTable tabla)
        {
            var columnas = tabla.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();

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

        private static string EscaparParaRowFilter(string valor)
        {
            return valor.Replace("'", "''");
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
