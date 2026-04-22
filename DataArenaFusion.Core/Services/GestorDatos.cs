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

        // OBJETO DE BLOQUEO PARA CONCURRENCIA (Indispensable para Singletons en Web)
        private readonly object _syncLock = new object();
        public object SyncRoot => _syncLock;

        public GestorDatos()
        {
            TablaActual = CrearTablaBase();
            RegistrosActuales = new List<Registro>();
            IndiceId = new Dictionary<int, Registro>();
        }

        public void CargarArchivo(string ruta)
        {
            lock (_syncLock)
            {
                Console.WriteLine($"[DIAGNÓSTICO] Iniciando carga de archivo: {Path.GetFileName(ruta)}");
                
                try
                {
                    var lector = LectorFactory.ObtenerLector(ruta);
                    var importacion = lector.Leer(ruta);
                    
                    Console.WriteLine($"[DIAGNÓSTICO] Lectura completada satisfactoriamente. Sincronizando con DataTable...");

                    AsegurarColumnaFuente();
                    string fuente = ObtenerNombreFuenteUnico(ruta);
                    
                    Console.WriteLine($"[DIAGNÓSTICO] Fuente determinada: {fuente}. Agregando filas...");
                    AgregarImportacion(importacion, fuente);
                    
                    importacion.Filas.Clear();
                    importacion.Encabezados.Clear();

                    Console.WriteLine("[DIAGNÓSTICO] Extrayendo registros...");
                    RegistrosActuales = ConvertirARegistros(TablaActual);
                    
                    Console.WriteLine("[DIAGNÓSTICO] Finalizando carga.");
                    ReconstruirIndice();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR CRÍTICO] {ex.Message}");
                    throw;
                }
                finally
                {
                    // Eliminamos GC.Collect forzado para evitar inestabilidad en el proceso
                }
            }
        }

        public void Limpiar()
        {
            lock (_syncLock)
            {
                Console.WriteLine("[DIAGNÓSTICO] Limpiando datos...");
                TablaActual = CrearTablaBase();
                RegistrosActuales = new List<Registro>();
                IndiceId.Clear();
            }
        }

        public void OrdenarAscendente()
        {
            lock (_syncLock)
            {
                OrdenadorDatos.BubbleSortPorValor(RegistrosActuales, true);
            }
        }

        public Dictionary<string, double> ObtenerDatosParaGrafica()
        {
            lock (_syncLock)
            {
                return Agrupador.SumarPorCategoria(RegistrosActuales);
            }
        }

        public void SincronizarTablaDesdeMemoria()
        {
            lock (_syncLock)
            {
                TablaActual.BeginLoadData();
                TablaActual.Rows.Clear();
                
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
                        if (TablaActual.Columns.Contains(extra.Key)) fila[extra.Key] = extra.Value;
                    }
                    TablaActual.Rows.Add(fila);
                }
                TablaActual.EndLoadData();
            }
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

                int id = 0;
                int.TryParse(valorIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);

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

                registros.Add(new Registro { Id = id, Categoria = valorCat, Valor = valor, Extras = extras });
            }

            return registros;
        }

        private void DetectarColumnas(DataTable tabla)
        {
            var headers = tabla.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            _colId = headers.FirstOrDefault(h => new[] { "id", "código", "item", "nro", "key", "code" }.Any(k => h.Contains(k, StringComparison.OrdinalIgnoreCase))) ?? "";
            _colCat = headers.FirstOrDefault(h => new[] { "nombre", "producto", "categoría", "descripción", "tipo", "name", "category", "description" }.Any(k => h.Contains(k, StringComparison.OrdinalIgnoreCase))) ?? "";
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
                        fila[encabezado] = filaImportada.TryGetValue(encabezado, out var valor) ? valor ?? string.Empty : string.Empty;
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
            Console.WriteLine("[DIAGNÓSTICO] Asegurando columna 'Fuente'...");
            if (!TablaActual.Columns.Contains("Fuente"))
            {
                var col = new DataColumn("Fuente", typeof(string));
                TablaActual.Columns.Add(col);
                col.SetOrdinal(0);
                Console.WriteLine("[DIAGNÓSTICO] Columna 'Fuente' creada.");
            }
        }

        private string ObtenerNombreFuenteUnico(string ruta)
        {
            Console.WriteLine("[DIAGNÓSTICO] Calculando nombre de fuente único...");
            var nombreBase = Path.GetFileName(ruta);
            var nombre = nombreBase;
            var contador = 2;

            while (TablaActual.Rows.Cast<DataRow>()
                .Any(fila => string.Equals(fila["Fuente"]?.ToString(), nombre, StringComparison.OrdinalIgnoreCase)))
            {
                var extension = Path.GetExtension(nombreBase);
                var sinExtension = Path.GetFileNameWithoutExtension(nombreBase);
                nombre = string.IsNullOrWhiteSpace(extension) ? $"{sinExtension}_{contador}" : $"{sinExtension}_{contador}{extension}";
                contador++;
            }

            return nombre;
        }

        private static DataTable CrearTablaBase() => new DataTable();

        private void ReconstruirIndice()
        {
            IndiceId.Clear();
            foreach (var reg in RegistrosActuales)
            {
                if (!IndiceId.ContainsKey(reg.Id)) IndiceId.Add(reg.Id, reg);
            }
        }
    }
}
