using Microsoft.AspNetCore.Mvc;
using DataArenaFusion.Services;
using System.Data;

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

        // --- INICIO DE LA CORRECCIÓN PARA ARCHIVOS GRANDES ---
        [HttpPost("Upload")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Archivo inválido o no se recibió.");

            var tempPath = Path.GetTempFileName() + Path.GetExtension(file.FileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                _gestor.CargarArchivo(tempPath);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            finally
            {
                if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
            }
        }
        // --- FIN DE LA CORRECCIÓN ---

        [HttpGet("GetData")]
        public IActionResult GetData()
        {
            var data = new List<Dictionary<string, object>>();
            var columns = new List<string>();

            if (_gestor.TablaActual != null && _gestor.TablaActual.Columns.Count > 0)
            {
                foreach (DataColumn col in _gestor.TablaActual.Columns)
                {
                    columns.Add(col.ColumnName);
                }

                // --- LÍMITE DE SEGURIDAD PARA EVITAR CRASH DEL SERVIDOR ---
                // Mandamos máximo 2,000 filas al frontend (el total de la tabla sigue intacto en memoria para las gráficas)
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

        [HttpPost("Ordenar")]
        public IActionResult Ordenar()
        {
            // The desktop button Ordenar invokes GestorDatos.OrdenarAscendente() which uses QuickSort
            _gestor.OrdenarAscendente();
            _gestor.SincronizarTablaDesdeMemoria();
            return Ok();
        }

        [HttpPost("Duplicados")]
        public IActionResult Duplicados([FromQuery] string columna)
        {
            if (string.IsNullOrWhiteSpace(columna)) return BadRequest("Columna requerida");
            var procesador = new DataArenaFusion.Processing.Procesadores.ProcesadorDuplicados(_gestor.ColId, _gestor.ColCat, _gestor.ColVal);
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

        [HttpPost("EnriquecerApi")]
        public async Task<IActionResult> EnriquecerApi()
        {
            if (_gestor.TablaActual.Rows.Count == 0) return BadRequest("No hay datos.");

            await DataEnricherService.EnriquecerDataTableAsync(_gestor.TablaActual);
            // After modifying DataTable, reload into records optionally or just leave table modified.
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