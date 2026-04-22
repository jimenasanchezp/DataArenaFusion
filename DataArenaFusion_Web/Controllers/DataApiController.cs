using Microsoft.AspNetCore.Mvc;
using DataArenaFusion.Core.Services;
using System.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace DataArenaFusion_Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataApiController : ControllerBase
    {
        private readonly GestorDatos _gestor;

        public DataApiController(GestorDatos gestor)
        {
            _gestor = gestor;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            Console.WriteLine("[UPLOAD] Entrando al método Upload. Petición recibida.");

            if (file == null || file.Length == 0)
            {
                Console.WriteLine("[UPLOAD ERROR] No se recibió ningún archivo o está vacío.");
                return BadRequest("No se recibió ningún archivo.");
            }

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "temp_uploads");
            string tempFolder = Path.Combine(rootPath, Guid.NewGuid().ToString());
            string tempPath = Path.Combine(tempFolder, file.FileName);

            try
            {
                if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
                Directory.CreateDirectory(tempFolder);

                Console.WriteLine($"[UPLOAD] Guardando archivo: {file.FileName} ({file.Length} bytes)");

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine("[UPLOAD] Archivo guardado físicamente. Iniciando procesamiento...");
                _gestor.CargarArchivo(tempPath);

                Console.WriteLine("[UPLOAD] Procesamiento completado con éxito.");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL UPLOAD ERROR] {ex.GetType().Name}: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    try { Directory.Delete(tempFolder, true); Console.WriteLine("[UPLOAD] Limpieza de temporales exitosa."); }
                    catch (Exception ex) { Console.WriteLine($"[UPLOAD WARNING] Error al limpiar: {ex.Message}"); }
                }
            }
        }

        [HttpGet("GetData")]
        public IActionResult GetData()
        {
            // PROTEGER LECTURA CONTRA ESCRITURAS SIMULTÁNEAS
            lock (_gestor.SyncRoot)
            {
                var data = new List<Dictionary<string, object>>();
                var columns = new List<string>();

                if (_gestor.TablaActual != null)
                {
                    foreach (DataColumn col in _gestor.TablaActual.Columns)
                    {
                        columns.Add(col.ColumnName);
                    }

                    int limite = Math.Min(_gestor.TablaActual.Rows.Count, 2000);

                    for (int i = 0; i < limite; i++)
                    {
                        var row = _gestor.TablaActual.Rows[i];
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in _gestor.TablaActual.Columns)
                        {
                            dict[col.ColumnName] = row[col] == DBNull.Value ? "" : row[col];
                        }
                        data.Add(dict);
                    }
                }

                return Ok(new { columns, data });
            }
        }

        [HttpPost("Ordenar")]
        public IActionResult Ordenar()
        {
            _gestor.OrdenarAscendente();
            _gestor.SincronizarTablaDesdeMemoria();
            return Ok();
        }

        [HttpPost("Duplicados")]
        public IActionResult Duplicados([FromQuery] string columna)
        {
            if (string.IsNullOrWhiteSpace(columna)) return BadRequest("Columna requerida");

            lock (_gestor.SyncRoot)
            {
                var procesador = new DataArenaFusion.Core.Processing.Procesadores.ProcesadorDuplicados(_gestor.ColId, _gestor.ColCat, _gestor.ColVal);
                var duplicados = procesador.Procesar(_gestor.RegistrosActuales, columna);

                var valoresRepetidos = new HashSet<string>();
                foreach (var d in duplicados)
                {
                    string val = "";
                    if (columna == _gestor.ColId) val = d.Id.ToString();
                    else if (columna == _gestor.ColCat) val = d.Categoria;
                    else if (columna == _gestor.ColVal) val = d.Valor.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    else { d.Extras.TryGetValue(columna, out val); }
                    if (val != null) valoresRepetidos.Add(val);
                }
                return Ok(valoresRepetidos);
            }
        }

        [HttpPost("EnriquecerApi")]
        public async Task<IActionResult> EnriquecerApi()
        {
            lock (_gestor.SyncRoot)
            {
                if (_gestor.TablaActual.Rows.Count == 0) return BadRequest("No hay datos.");
            }

            // Nota: EnriquecerDataTableAsync ya tiene su propio manejo interno de hilos 
            // a través de Task.Run, pero lo ideal es bloquear el DataTable durante la operación.
            await DataEnricherService.EnriquecerDataTableAsync(_gestor.TablaActual);
            return Ok();
        }

        [HttpPost("Limpiar")]
        public IActionResult Limpiar()
        {
            _gestor.Limpiar();
            return Ok();
        }
    }
}