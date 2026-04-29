using DataArenaFusion.Core.Services;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            Console.WriteLine("[UPLOAD] Entrando al metodo Upload. Peticion recibida.");

            if (file == null || file.Length == 0)
            {
                Console.WriteLine("[UPLOAD ERROR] No se recibio ningun archivo o esta vacio.");
                return BadRequest("No se recibio ningun archivo.");
            }

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "temp_uploads");
            string tempFolder = Path.Combine(rootPath, Guid.NewGuid().ToString());
            string tempPath = Path.Combine(tempFolder, file.FileName);

            try
            {
                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                Directory.CreateDirectory(tempFolder);

                Console.WriteLine($"[UPLOAD] Guardando archivo: {file.FileName} ({file.Length} bytes)");

                await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine("[UPLOAD] Archivo guardado fisicamente. Iniciando procesamiento...");
                _gestor.CargarArchivo(tempPath);

                Console.WriteLine("[UPLOAD] Procesamiento completado con exito.");
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
                    try
                    {
                        Directory.Delete(tempFolder, true);
                        Console.WriteLine("[UPLOAD] Limpieza de temporales exitosa.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UPLOAD WARNING] Error al limpiar: {ex.Message}");
                    }
                }
            }
        }

        [HttpGet("GetData")]
        public IActionResult GetData([FromQuery] int page = 1, [FromQuery] int pageSize = 200)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 200;
            }

            lock (_gestor.SyncRoot)
            {
                var data = new List<Dictionary<string, object>>();
                var columns = new List<string>();
                var totalCount = _gestor.TablaActual?.Rows.Count ?? 0;

                if (_gestor.TablaActual != null)
                {
                    foreach (DataColumn col in _gestor.TablaActual.Columns)
                    {
                        columns.Add(col.ColumnName);
                    }

                    var skip = (page - 1) * pageSize;
                    if (skip < totalCount)
                    {
                        var take = Math.Min(pageSize, totalCount - skip);

                        for (var i = skip; i < skip + take; i++)
                        {
                            var row = _gestor.TablaActual.Rows[i];
                            var dict = new Dictionary<string, object>();

                            foreach (DataColumn col in _gestor.TablaActual.Columns)
                            {
                                dict[col.ColumnName] = row[col] == DBNull.Value ? string.Empty : row[col];
                            }

                            data.Add(dict);
                        }
                    }
                }

                var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
                return Ok(new { columns, data, totalCount, page, pageSize, totalPages });
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
            if (string.IsNullOrWhiteSpace(columna))
            {
                return BadRequest("Columna requerida");
            }

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
                    else if (d.Extras.TryGetValue(columna, out var extraVal)) val = extraVal ?? "";
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        valoresRepetidos.Add(val);
                    }
                }

                return Ok(valoresRepetidos);
            }
        }

        [HttpPost("EnriquecerApi")]
        public IActionResult EnriquecerApi()
        {
            lock (_gestor.SyncRoot)
            {
                if (_gestor.TablaActual.Rows.Count == 0)
                {
                    return BadRequest("No hay datos.");
                }

                var resumen = DataEnricherService.EnriquecerDataTable(_gestor.TablaActual);
                _gestor.SincronizarListaDesdeTabla();
                return Ok(resumen);
            }
        }

        [HttpPost("Agrupar")]
        public IActionResult Agrupar([FromQuery] string columna)
        {
            if (string.IsNullOrWhiteSpace(columna))
            {
                return BadRequest("Columna requerida");
            }

            lock (_gestor.SyncRoot)
            {
                var procesador = new DataArenaFusion.Core.Processing.Procesadores.ProcesadorAgrupamiento(_gestor.ColId, _gestor.ColCat, _gestor.ColVal);
                var agrupados = procesador.Procesar(_gestor.RegistrosActuales, columna);

                return Ok(agrupados);
            }
        }

        [HttpPost("Limpiar")]
        public IActionResult Limpiar()
        {
            _gestor.Limpiar();
            return Ok();
        }

        [HttpPost("TestConnection")]
        public IActionResult TestConnection([FromBody] DataArenaFusion.Core.Models.DatabaseConnectionSettings settings)
        {
            try
            {
                var service = DataArenaFusion.Core.Services.Database.DatabaseConnectionServiceFactory.Create(settings.Provider);
                if (service.TryTestConnection(settings, out string mensaje))
                {
                    return Ok(new { success = true, message = "Conexion exitosa" });
                }

                return BadRequest(new { success = false, message = mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Connect")]
        public IActionResult Connect([FromBody] DataArenaFusion.Core.Models.DatabaseConnectionSettings settings)
        {
            try
            {
                var service = DataArenaFusion.Core.Services.Database.DatabaseConnectionServiceFactory.Create(settings.Provider);
                string mensaje = service.MigrarDatos(settings, _gestor.TablaActual);

                if (mensaje == "OK")
                {
                    _gestor.SincronizarListaDesdeTabla();
                    return Ok(new { success = true, message = "Datos importados correctamente" });
                }

                return BadRequest(new { success = false, message = mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
