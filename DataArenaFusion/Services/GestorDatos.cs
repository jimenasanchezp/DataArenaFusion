using System.Data;
using System.Globalization;
using DataArenaFusion.Data;
using DataArenaFusion.Models;
using DataArenaFusion.Processing.Algoritmos;

namespace DataArenaFusion.Services
{
    public class GestorDatos
    {
        public DataTable TablaActual { get; private set; }
        public List<Registro> RegistrosActuales { get; private set; }
        public Dictionary<int, Registro> IndiceId { get; private set; }

        public GestorDatos()
        {
            TablaActual = CrearTablaBase();
            RegistrosActuales = new List<Registro>();
            IndiceId = new Dictionary<int, Registro>();
        }

        public void CargarArchivo(string ruta)
        {
            var lector = LectorFactory.ObtenerLector(ruta);
            var importacion = lector.Leer(ruta);

            // Llamar al nuevo servicio inteligente de APIs
            DataEnricherService.EnriquecerTabla(importacion);

            AsegurarColumnaFuente();
            var fuente = ObtenerNombreFuenteUnico(ruta);

            AgregarImportacion(importacion, fuente);
            RegistrosActuales = ConvertirARegistros(TablaActual);
            ReconstruirIndice();
        }

        public void Limpiar()
        {
            TablaActual = CrearTablaBase();
            RegistrosActuales = new List<Registro>();
            IndiceId.Clear();
        }

        public void OrdenarAscendente()
        {
            OrdenadorDatos.BubbleSortPorValor(RegistrosActuales, true);
        }

        public Dictionary<string, double> ObtenerDatosParaGrafica()
        {
            return Agrupador.SumarPorCategoria(RegistrosActuales);
        }

        public int ContarDuplicados()
        {
            return DetectorDuplicados.ObtenerDuplicados(RegistrosActuales).Count;
        }

        private void AgregarImportacion(TablaImportada importacion, string fuente)
        {
            AsegurarColumnas(importacion.Encabezados);

            TablaActual.BeginLoadData();
            try
            {
                foreach (var filaImportada in importacion.Filas)
                {
                    var fila = TablaActual.NewRow();
                    fila["Fuente"] = fuente;

                    foreach (var encabezado in importacion.Encabezados)
                    {
                        fila[encabezado] = filaImportada.TryGetValue(encabezado, out var valor)
                            ? valor ?? string.Empty
                            : string.Empty;
                    }

                    TablaActual.Rows.Add(fila);
                }
            }
            finally
            {
                TablaActual.EndLoadData();
            }
        }

        private void AsegurarColumnas(IEnumerable<string> encabezados)
        {
            foreach (var encabezado in encabezados)
            {
                if (!TablaActual.Columns.Contains(encabezado))
                {
                    TablaActual.Columns.Add(encabezado, typeof(string));
                }
            }
        }

        private void AsegurarColumnaFuente()
        {
            if (!TablaActual.Columns.Contains("Fuente"))
            {
                TablaActual.Columns.Add("Fuente", typeof(string));
                TablaActual.Columns["Fuente"]!.SetOrdinal(0);
            }
        }

        private string ObtenerNombreFuenteUnico(string ruta)
        {
            var nombreBase = Path.GetFileName(ruta);
            var nombre = nombreBase;
            var contador = 2;

            while (TablaActual.Rows.Cast<DataRow>()
                .Any(fila => string.Equals(fila["Fuente"]?.ToString(), nombre, StringComparison.OrdinalIgnoreCase)))
            {
                var extension = Path.GetExtension(nombreBase);
                var sinExtension = Path.GetFileNameWithoutExtension(nombreBase);
                nombre = string.IsNullOrWhiteSpace(extension)
                    ? $"{sinExtension}_{contador}"
                    : $"{sinExtension}_{contador}{extension}";
                contador++;
            }

            return nombre;
        }

        private static DataTable CrearTablaBase()
        {
            return new DataTable();
        }

        private void ReconstruirIndice()
        {
            IndiceId.Clear();

            foreach (var reg in RegistrosActuales)
            {
                if (!IndiceId.ContainsKey(reg.Id))
                {
                    IndiceId.Add(reg.Id, reg);
                }
            }
        }

        private static List<Registro> ConvertirARegistros(DataTable tabla)
        {
            var registros = new List<Registro>();

            if (!tabla.Columns.Contains("Id") ||
                !tabla.Columns.Contains("Categoria") ||
                !tabla.Columns.Contains("Valor"))
            {
                return registros;
            }

            foreach (DataRow fila in tabla.Rows)
            {
                var valorId = fila["Id"]?.ToString();
                var valorCategoria = fila["Categoria"]?.ToString() ?? string.Empty;
                var valorImporte = fila["Valor"]?.ToString();

                if (!int.TryParse(valorId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) &&
                    !int.TryParse(valorId, NumberStyles.Integer, CultureInfo.CurrentCulture, out id))
                {
                    continue;
                }

                if (!double.TryParse(valorImporte, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var valor) &&
                    !double.TryParse(valorImporte, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out valor))
                {
                    continue;
                }

                var extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (DataColumn columna in tabla.Columns)
                {
                    if (EsColumnaPrincipal(columna.ColumnName))
                    {
                        continue;
                    }

                    extras[columna.ColumnName] = fila[columna.ColumnName]?.ToString() ?? string.Empty;
                }

                registros.Add(new Registro
                {
                    Id = id,
                    Categoria = valorCategoria,
                    Valor = valor,
                    Extras = extras
                });
            }

            return registros;
        }

        private static bool EsColumnaPrincipal(string encabezado)
        {
            return string.Equals(encabezado, "Id", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(encabezado, "Categoria", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(encabezado, "Valor", StringComparison.OrdinalIgnoreCase);
        }
    }
}
