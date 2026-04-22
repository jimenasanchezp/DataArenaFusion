using System.Data;
using System.Globalization;
using DataArenaFusion.Core.Data;
using DataArenaFusion.Core.Models;
using DataArenaFusion.Core.Processing.Algoritmos;

namespace DataArenaFusion.Core.Services
{
    public class GestorDatos
    {
        public DataTable TablaActual { get; private set; }
        public List<Registro> RegistrosActuales { get; private set; }
        public Dictionary<int, Registro> IndiceId { get; private set; }

        public string ColId => _colId;
        public string ColCat => _colCat;
        public string ColVal => _colVal;

        private string _colId = "Id";
        private string _colCat = "Categoria";
        private string _colVal = "Valor";

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

            // Llamar al nuevo servicio inteligente de APIs (DESACTIVADO POR RENDIMIENTO)
            // DataEnricherService.EnriquecerTabla(importacion);

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

        public void FiltrarDatos(string columna, string valor)
        {
            if (string.IsNullOrWhiteSpace(columna) || string.IsNullOrWhiteSpace(valor) || columna == "Todas las columnas")
            {
                TablaActual.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                // Escape simple quotes and avoid injection by doing basic escaping
                var valorSeguro = valor.Replace("'", "''");
                TablaActual.DefaultView.RowFilter = $"CONVERT([{columna}], 'System.String') LIKE '%{valorSeguro}%'";
            }
        }

        public Dictionary<string, double> ObtenerDatosParaGrafica()
        {
            return Agrupador.SumarPorCategoria(RegistrosActuales);
        }

        public int ContarDuplicados()
        {
            return DetectorDuplicados.ObtenerDuplicados(RegistrosActuales).Count;
        }

        public List<Registro> ObtenerListaDuplicados()
        {
            return DetectorDuplicados.ObtenerDuplicados(RegistrosActuales);
        }

        public void SincronizarTablaDesdeMemoria()
        {
            TablaActual.BeginLoadData();
            TablaActual.Rows.Clear();
            
            // Cachear el esquema de la tabla
            bool hasId = TablaActual.Columns.Contains(_colId);
            bool hasCat = TablaActual.Columns.Contains(_colCat);
            bool hasVal = TablaActual.Columns.Contains(_colVal);
            
            foreach (var reg in RegistrosActuales)
            {
                var fila = TablaActual.NewRow();
                
                if (hasId) fila[_colId] = reg.Id.ToString();
                if (hasCat) fila[_colCat] = reg.Categoria;
                if (hasVal) fila[_colVal] = reg.Valor.ToString(CultureInfo.InvariantCulture);

                foreach (var extra in reg.Extras)
                {
                    if (TablaActual.Columns.Contains(extra.Key))
                    {
                        fila[extra.Key] = extra.Value;
                    }
                }
                TablaActual.Rows.Add(fila);
            }
            
            TablaActual.EndLoadData();
        }

        private List<Registro> ConvertirARegistros(DataTable tabla)
        {
            var registros = new List<Registro>();
            DetectarColumnas(tabla);

            foreach (DataRow fila in tabla.Rows)
            {
                var valorIdStr = !string.IsNullOrEmpty(_colId) ? fila[_colId]?.ToString() : "0";
                var valorCat = !string.IsNullOrEmpty(_colCat) ? fila[_colCat]?.ToString() ?? "" : "General";
                var valorNumStr = !string.IsNullOrEmpty(_colVal) ? fila[_colVal]?.ToString() : "0";

                // Intentar Parsear ID (si existe)
                int id = 0;
                int.TryParse(valorIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);

                // Intentar Parsear Valor
                double valor = 0;
                if (!string.IsNullOrEmpty(valorNumStr))
                {
                    if (!double.TryParse(valorNumStr, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out valor))
                    {
                        double.TryParse(valorNumStr, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out valor);
                    }
                }

                var extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (DataColumn columna in tabla.Columns)
                {
                    if (EsColumnaPrincipal(columna.ColumnName)) continue;
                    extras[columna.ColumnName] = fila[columna.ColumnName]?.ToString() ?? string.Empty;
                }

                registros.Add(new Registro
                {
                    Id = id,
                    Categoria = valorCat,
                    Valor = valor,
                    Extras = extras
                });
            }

            return registros;
        }

        private void DetectarColumnas(DataTable tabla)
        {
            var headers = tabla.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

            // Buscar ID
            _colId = headers.FirstOrDefault(h => new[] { "id", "código", "item", "nro", "key", "code" }.Any(k => h.Contains(k, StringComparison.OrdinalIgnoreCase))) ?? "";
            
            // Buscar Categoria
            _colCat = headers.FirstOrDefault(h => new[] { "nombre", "producto", "categoría", "descripción", "tipo", "name", "category", "description" }.Any(k => h.Contains(k, StringComparison.OrdinalIgnoreCase))) ?? "";

            // Buscar Valor
            _colVal = headers.FirstOrDefault(h => new[] { "precio", "monto", "cantidad", "valor", "total", "price", "amount", "value", "quantity" }.Any(k => h.Contains(k, StringComparison.OrdinalIgnoreCase))) ?? "";
        }

        private bool EsColumnaPrincipal(string encabezado)
        {
            return (encabezado == _colId && !string.IsNullOrEmpty(_colId)) || 
                   (encabezado == _colCat && !string.IsNullOrEmpty(_colCat)) || 
                   (encabezado == _colVal && !string.IsNullOrEmpty(_colVal));
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
    }
}
