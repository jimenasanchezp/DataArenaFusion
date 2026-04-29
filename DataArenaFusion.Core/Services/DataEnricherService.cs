using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DataArenaFusion.Core.Services
{
    public sealed class EnrichmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RowsProcessed { get; set; }
        public int RowsUpdated { get; set; }
        public int ApiCalls { get; set; }
        public List<string> AddedColumns { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class DataEnricherService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        static DataEnricherService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DataArenaFusionApp/1.0");
        }

        public static void EnriquecerTabla(DataArenaFusion.Core.Models.TablaImportada importacion)
        {
            EnriquecerDivisas(importacion);
            EnriquecerMapas(importacion);
        }

        public static EnrichmentResult EnriquecerDataTable(DataTable tabla, Action<int, int>? progressCallback = null)
        {
            var result = new EnrichmentResult();

            if (tabla == null)
            {
                result.Message = "No se recibio ninguna tabla para enriquecer.";
                return result;
            }

            result.RowsProcessed = tabla.Rows.Count;

            try
            {
                var columnasAntes = tabla.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                EnriquecerDivisasDataTable(tabla, result);
                EnriquecerMapasDataTable(tabla, result, progressCallback);

                var columnasDespues = tabla.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToList();

                result.AddedColumns = columnasDespues
                    .Where(c => !columnasAntes.Contains(c))
                    .ToList();

                result.Success = true;
                result.Message = result.AddedColumns.Count > 0
                    ? $"Enriquecimiento completado. Se agregaron {result.AddedColumns.Count} columnas nuevas."
                    : "Enriquecimiento completado, pero no se detectaron columnas compatibles para agregar datos.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"No se pudo enriquecer la tabla: {ex.Message}";
            }

            return result;
        }

        public static Task<EnrichmentResult> EnriquecerDataTableAsync(DataTable tabla, Action<int, int>? progressCallback = null)
        {
            return Task.FromResult(EnriquecerDataTable(tabla, progressCallback));
        }

        private static void EnriquecerDivisasDataTable(DataTable tabla, EnrichmentResult result)
        {
            var palabrasClaveDivisa = new[] { "precio", "costo", "monto", "valor", "total", "price", "cost", "amount", "usd", "eur" };
            var columnasObjetivo = tabla.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .Where(c => palabrasClaveDivisa.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!columnasObjetivo.Any())
            {
                return;
            }

            result.ApiCalls++;

            try
            {
                var response = _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    result.Warnings.Add("No se pudo consultar la API de divisas.");
                    return;
                }

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("rates", out var rates))
                {
                    result.Warnings.Add("La API de divisas no devolvio tasas validas.");
                    return;
                }

                var usdToMxn = rates.GetProperty("MXN").GetDouble();
                var eurToUsd = 1.0 / rates.GetProperty("EUR").GetDouble();
                var gbpToUsd = 1.0 / rates.GetProperty("GBP").GetDouble();

                foreach (var columna in columnasObjetivo)
                {
                    var monedaBase = "USD";
                    int muestras = 0;

                    foreach (DataRow fila in tabla.Rows)
                    {
                        if (muestras >= 10) break;

                        var val = fila[columna];
                        if (val == null || val == DBNull.Value)
                        {
                            continue;
                        }

                        var texto = val.ToString()?.ToUpperInvariant() ?? string.Empty;
                        if (texto.Contains("€") || texto.Contains("EUR")) monedaBase = "EUR";
                        else if (texto.Contains("£") || texto.Contains("GBP")) monedaBase = "GBP";
                        else if (texto.Contains("$") || texto.Contains("USD")) monedaBase = "USD";
                        muestras++;
                    }

                    double tasaConversion = usdToMxn;
                    if (monedaBase == "EUR") tasaConversion = eurToUsd * usdToMxn;
                    if (monedaBase == "GBP") tasaConversion = gbpToUsd * usdToMxn;

                    var nombreNuevaColumna = $"{columna} (MXN)";
                    if (!tabla.Columns.Contains(nombreNuevaColumna))
                    {
                        tabla.Columns.Add(nombreNuevaColumna, typeof(string));
                    }

                    foreach (DataRow fila in tabla.Rows)
                    {
                        var val = fila[columna];
                        if (val != null && val != DBNull.Value)
                        {
                            var textoLimpio = Regex.Replace(val.ToString() ?? string.Empty, @"[^\d.-]", "");
                            if (double.TryParse(textoLimpio, out double cantidad))
                            {
                                double convertido = cantidad * tasaConversion;
                                double truncado = Math.Truncate(convertido * 100) / 100;
                                fila[nombreNuevaColumna] = "$" + truncado.ToString("0.00");
                            }
                            else
                            {
                                fila[nombreNuevaColumna] = "$0.00";
                            }
                        }
                        else
                        {
                            fila[nombreNuevaColumna] = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add("Error al enriquecer divisas: " + ex.Message);
            }
        }

        private static void EnriquecerMapasDataTable(DataTable tabla, EnrichmentResult result, Action<int, int>? progressCallback = null)
        {
            var palabrasClaveCiudad = new[] { "ciudad", "city", "ubicacion", "location", "localidad", "municipio" };
            var columnaCiudad = tabla.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .FirstOrDefault(c => palabrasClaveCiudad.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)));

            if (columnaCiudad == null)
            {
                return;
            }

            if (!tabla.Columns.Contains("Latitud")) tabla.Columns.Add("Latitud", typeof(string));
            if (!tabla.Columns.Contains("Longitud")) tabla.Columns.Add("Longitud", typeof(string));
            if (!tabla.Columns.Contains("Pais")) tabla.Columns.Add("Pais", typeof(string));
            if (!tabla.Columns.Contains("Estado")) tabla.Columns.Add("Estado", typeof(string));
            if (!tabla.Columns.Contains("Municipio")) tabla.Columns.Add("Municipio", typeof(string));
            if (!tabla.Columns.Contains("Lugar")) tabla.Columns.Add("Lugar", typeof(string));

            result.ApiCalls++;

            var cache = new Dictionary<string, (string Lat, string Lon, string Pais, string Estado, string Municipio, string Lugar)>(StringComparer.OrdinalIgnoreCase);
            int total = tabla.Rows.Count;
            int actual = 0;

            foreach (DataRow fila in tabla.Rows)
            {
                actual++;
                var ciudadVal = fila[columnaCiudad];
                if (ciudadVal == null || ciudadVal == DBNull.Value)
                {
                    progressCallback?.Invoke(actual, total);
                    continue;
                }

                var ciudad = ciudadVal.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(ciudad))
                {
                    progressCallback?.Invoke(actual, total);
                    continue;
                }

                if (!cache.ContainsKey(ciudad))
                {
                    try
                    {
                        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(ciudad)}&format=jsonv2&addressdetails=1&limit=1";
                        var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();

                        if (response.IsSuccessStatusCode)
                        {
                            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            using var doc = JsonDocument.Parse(json);

                            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                            {
                                var item = doc.RootElement[0];
                                var lat = item.TryGetProperty("lat", out var latEl) ? latEl.GetString() ?? string.Empty : string.Empty;
                                var lon = item.TryGetProperty("lon", out var lonEl) ? lonEl.GetString() ?? string.Empty : string.Empty;
                                var display = item.TryGetProperty("display_name", out var displayEl) ? displayEl.GetString() ?? ciudad : ciudad;

                                string pais = string.Empty;
                                string estado = string.Empty;
                                string municipio = string.Empty;

                                if (item.TryGetProperty("address", out var address))
                                {
                                    pais = ObtenerValor(address, "country");
                                    estado = ObtenerValor(address, "state");
                                    municipio = ObtenerValor(address, "city");
                                    if (string.IsNullOrWhiteSpace(municipio)) municipio = ObtenerValor(address, "town");
                                    if (string.IsNullOrWhiteSpace(municipio)) municipio = ObtenerValor(address, "village");
                                    if (string.IsNullOrWhiteSpace(municipio)) municipio = ObtenerValor(address, "county");
                                }

                                cache[ciudad] = (lat, lon, pais, estado, municipio, display);
                            }
                            else
                            {
                                cache[ciudad] = (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ciudad);
                            }
                        }
                        else
                        {
                            cache[ciudad] = (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ciudad);
                            result.Warnings.Add($"No se pudo geocodificar '{ciudad}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        cache[ciudad] = (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ciudad);
                        result.Warnings.Add($"Error al geocodificar '{ciudad}': {ex.Message}");
                    }

                    Thread.Sleep(1100);
                }

                var coords = cache[ciudad];
                fila["Latitud"] = coords.Lat;
                fila["Longitud"] = coords.Lon;
                fila["Pais"] = coords.Pais;
                fila["Estado"] = coords.Estado;
                fila["Municipio"] = coords.Municipio;
                fila["Lugar"] = coords.Lugar;
                result.RowsUpdated++;

                progressCallback?.Invoke(actual, total);
            }
        }

        private static string ObtenerValor(JsonElement address, string nombrePropiedad)
        {
            return address.TryGetProperty(nombrePropiedad, out var prop) ? prop.GetString() ?? string.Empty : string.Empty;
        }

        private static void EnriquecerDivisas(DataArenaFusion.Core.Models.TablaImportada importacion)
        {
            try
            {
                var palabrasClaveDivisa = new[] { "precio", "costo", "monto", "valor", "total", "price", "cost", "amount", "usd", "eur" };
                var columnasObjetivo = importacion.Encabezados
                    .Where(c => palabrasClaveDivisa.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (!columnasObjetivo.Any()) return;

                var response = _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode) return;

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("rates", out var rates)) return;

                var usdToMxn = rates.GetProperty("MXN").GetDouble();
                var eurToUsd = 1.0 / rates.GetProperty("EUR").GetDouble();
                var gbpToUsd = 1.0 / rates.GetProperty("GBP").GetDouble();

                foreach (var columna in columnasObjetivo)
                {
                    var monedaBase = "USD";

                    foreach (var fila in importacion.Filas.Take(10))
                    {
                        if (fila.TryGetValue(columna, out var val) && val != null)
                        {
                            var texto = val.ToString()?.ToUpperInvariant() ?? string.Empty;
                            if (texto.Contains("€") || texto.Contains("EUR")) monedaBase = "EUR";
                            else if (texto.Contains("£") || texto.Contains("GBP")) monedaBase = "GBP";
                            else if (texto.Contains("$") || texto.Contains("USD")) monedaBase = "USD";
                        }
                    }

                    double tasaConversion = usdToMxn;
                    if (monedaBase == "EUR") tasaConversion = eurToUsd * usdToMxn;
                    if (monedaBase == "GBP") tasaConversion = gbpToUsd * usdToMxn;

                    var nombreNuevaColumna = $"{columna} (MXN)";
                    if (!importacion.Encabezados.Contains(nombreNuevaColumna))
                    {
                        importacion.Encabezados.Add(nombreNuevaColumna);
                    }

                    foreach (var fila in importacion.Filas)
                    {
                        if (fila.TryGetValue(columna, out var val) && val != null)
                        {
                            var textoLimpio = Regex.Replace(val.ToString() ?? string.Empty, @"[^\d.-]", "");
                            if (double.TryParse(textoLimpio, out double cantidad))
                            {
                                fila[nombreNuevaColumna] = Math.Round(cantidad * tasaConversion, 2).ToString("0.00");
                            }
                            else
                            {
                                fila[nombreNuevaColumna] = "0.00";
                            }
                        }
                        else
                        {
                            fila[nombreNuevaColumna] = string.Empty;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static void EnriquecerMapas(DataArenaFusion.Core.Models.TablaImportada importacion)
        {
            try
            {
                var palabrasClaveCiudad = new[] { "ciudad", "city", "ubicacion", "location", "localidad", "municipio" };
                var columnaCiudad = importacion.Encabezados
                    .FirstOrDefault(c => palabrasClaveCiudad.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)));

                if (columnaCiudad == null) return;

                if (!importacion.Encabezados.Contains("Latitud")) importacion.Encabezados.Add("Latitud");
                if (!importacion.Encabezados.Contains("Longitud")) importacion.Encabezados.Add("Longitud");
                if (!importacion.Encabezados.Contains("Pais")) importacion.Encabezados.Add("Pais");
                if (!importacion.Encabezados.Contains("Estado")) importacion.Encabezados.Add("Estado");
                if (!importacion.Encabezados.Contains("Municipio")) importacion.Encabezados.Add("Municipio");
                if (!importacion.Encabezados.Contains("Lugar")) importacion.Encabezados.Add("Lugar");

                var cache = new Dictionary<string, (string Lat, string Lon, string Pais, string Estado, string Municipio, string Lugar)>(StringComparer.OrdinalIgnoreCase);

                foreach (var fila in importacion.Filas)
                {
                    if (fila.TryGetValue(columnaCiudad, out var ciudadVal) && ciudadVal != null)
                    {
                        var ciudad = ciudadVal.ToString()?.Trim() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(ciudad)) continue;

                        if (!cache.ContainsKey(ciudad))
                        {
                            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(ciudad)}&format=jsonv2&addressdetails=1&limit=1";
                            var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                using var doc = JsonDocument.Parse(json);
                                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                                {
                                    var item = doc.RootElement[0];
                                    var lat = item.TryGetProperty("lat", out var latEl) ? latEl.GetString() ?? string.Empty : string.Empty;
                                    var lon = item.TryGetProperty("lon", out var lonEl) ? lonEl.GetString() ?? string.Empty : string.Empty;
                                    var display = item.TryGetProperty("display_name", out var displayEl) ? displayEl.GetString() ?? ciudad : ciudad;

                                    string pais = string.Empty;
                                    string estado = string.Empty;
                                    string municipio = string.Empty;

                                    if (item.TryGetProperty("address", out var address))
                                    {
                                        pais = ObtenerValor(address, "country");
                                        estado = ObtenerValor(address, "state");
                                        municipio = ObtenerValor(address, "city");
                                        if (string.IsNullOrWhiteSpace(municipio)) municipio = ObtenerValor(address, "town");
                                        if (string.IsNullOrWhiteSpace(municipio)) municipio = ObtenerValor(address, "village");
                                        if (string.IsNullOrWhiteSpace(municipio)) municipio = ObtenerValor(address, "county");
                                    }

                                    cache[ciudad] = (lat, lon, pais, estado, municipio, display);
                                }
                                else
                                {
                                    cache[ciudad] = (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ciudad);
                                }
                            }
                            else
                            {
                                cache[ciudad] = (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ciudad);
                            }

                            Thread.Sleep(1100);
                        }

                        var coords = cache[ciudad];
                        fila["Latitud"] = coords.Lat;
                        fila["Longitud"] = coords.Lon;
                        fila["Pais"] = coords.Pais;
                        fila["Estado"] = coords.Estado;
                        fila["Municipio"] = coords.Municipio;
                        fila["Lugar"] = coords.Lugar;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
