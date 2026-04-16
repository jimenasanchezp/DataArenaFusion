using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DataArenaFusion.Services
{
    public class DataEnricherService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        static DataEnricherService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DataArenaFusionApp/1.0");
        }

        public static void EnriquecerTabla(DataArenaFusion.Models.TablaImportada importacion)
        {
            EnriquecerDivisas(importacion);
            EnriquecerMapas(importacion);
        }

        public static async Task EnriquecerDataTableAsync(DataTable tabla, Action<int, int> progressCallback = null)
        {
            await Task.Run(() => 
            {
                EnriquecerDivisasDataTable(tabla);
                EnriquecerMapasDataTable(tabla, progressCallback);
            });
        }

        private static void EnriquecerDivisasDataTable(DataTable tabla)
        {
            try
            {
                var palabrasClaveDivisa = new[] { "precio", "costo", "monto", "valor", "total", "price", "cost", "amount", "usd", "eur" };
                var columnasObjetivo = tabla.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .Where(c => palabrasClaveDivisa.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (!columnasObjetivo.Any()) return;

                var response = _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD").Result;
                if (!response.IsSuccessStatusCode) return;

                var json = response.Content.ReadAsStringAsync().Result;
                var root = JsonSerializer.Deserialize<JsonElement>(json);
                var rates = root.GetProperty("rates");
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
                        if (val != null && val != DBNull.Value)
                        {
                            var texto = val.ToString().ToUpperInvariant();
                            if (texto.Contains("€") || texto.Contains("EUR")) monedaBase = "EUR";
                            else if (texto.Contains("£") || texto.Contains("GBP")) monedaBase = "GBP";
                            else if (texto.Contains("$") || texto.Contains("USD")) monedaBase = "USD";
                            muestras++;
                        }
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
                            var textoLimpio = Regex.Replace(val.ToString(), @"[^\d.-]", "");
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
            catch { }
        }

        private static void EnriquecerMapasDataTable(DataTable tabla, Action<int, int> progressCallback = null)
        {
            try
            {
                var palabrasClaveCiudad = new[] { "ciudad", "city", "ubicacion", "location" };
                var columnaCiudad = tabla.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .FirstOrDefault(c => palabrasClaveCiudad.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)));

                if (columnaCiudad == null) return;

                if (!tabla.Columns.Contains("Latitud")) tabla.Columns.Add("Latitud", typeof(string));
                if (!tabla.Columns.Contains("Longitud")) tabla.Columns.Add("Longitud", typeof(string));

                var cache = new Dictionary<string, (string Lat, string Lon)>(StringComparer.OrdinalIgnoreCase);

                int total = tabla.Rows.Count;
                int actual = 0;

                foreach (DataRow fila in tabla.Rows)
                {
                    actual++;
                    var ciudadVal = fila[columnaCiudad];
                    if (ciudadVal != null && ciudadVal != DBNull.Value)
                    {
                        var ciudad = ciudadVal.ToString().Trim();
                        if (string.IsNullOrWhiteSpace(ciudad)) continue;

                        if (!cache.ContainsKey(ciudad))
                        {
                            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(ciudad)}&format=json&limit=1";
                            var response = _httpClient.GetAsync(url).Result;
                            
                            if (response.IsSuccessStatusCode)
                            {
                                var json = response.Content.ReadAsStringAsync().Result;
                                using var doc = JsonDocument.Parse(json);
                                if (doc.RootElement.GetArrayLength() > 0)
                                {
                                    var result = doc.RootElement[0];
                                    var lat = result.GetProperty("lat").GetString() ?? "";
                                    var lon = result.GetProperty("lon").GetString() ?? "";
                                    cache[ciudad] = (lat, lon);
                                }
                                else
                                {
                                    cache[ciudad] = ("", "");
                                }
                            }
                            else
                            {
                                cache[ciudad] = ("", "");
                            }
                            
                            Thread.Sleep(1000); 
                        }

                        var coords = cache[ciudad];
                        fila["Latitud"] = coords.Lat;
                        fila["Longitud"] = coords.Lon;
                    }
                    progressCallback?.Invoke(actual, total);
                }
            }
            catch { }
        }

        private static void EnriquecerDivisas(DataArenaFusion.Models.TablaImportada importacion)
        {
            try
            {
                var palabrasClaveDivisa = new[] { "precio", "costo", "monto", "valor", "total", "price", "cost", "amount", "usd", "eur" };
                var columnasObjetivo = importacion.Encabezados
                    .Where(c => palabrasClaveDivisa.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (!columnasObjetivo.Any()) return;

                // Download rates once
                var response = _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD").Result;
                if (!response.IsSuccessStatusCode) return;

                var json = response.Content.ReadAsStringAsync().Result;
                var root = JsonSerializer.Deserialize<JsonElement>(json);
                var rates = root.GetProperty("rates");
                var usdToMxn = rates.GetProperty("MXN").GetDouble();
                var eurToUsd = 1.0 / rates.GetProperty("EUR").GetDouble();
                var gbpToUsd = 1.0 / rates.GetProperty("GBP").GetDouble();

                foreach (var columna in columnasObjetivo)
                {
                    // Determinar moneda de la columna muestreando las primeras 10 filas
                    var monedaBase = "USD"; // por defecto
                    int muestras = 0;
                    
                    foreach (var fila in importacion.Filas.Take(10))
                    {
                        if (fila.TryGetValue(columna, out var val) && val != null)
                        {
                            var texto = val.ToString().ToUpperInvariant();
                            if (texto.Contains("€") || texto.Contains("EUR")) monedaBase = "EUR";
                            else if (texto.Contains("£") || texto.Contains("GBP")) monedaBase = "GBP";
                            else if (texto.Contains("$") || texto.Contains("USD")) monedaBase = "USD";
                            muestras++;
                        }
                    }

                    // Tasa de conversion a MXN
                    double tasaConversion = usdToMxn;
                    if (monedaBase == "EUR") tasaConversion = eurToUsd * usdToMxn;
                    if (monedaBase == "GBP") tasaConversion = gbpToUsd * usdToMxn;

                    // Crear e insertar nueva columna
                    var nombreNuevaColumna = $"{columna} (MXN)";
                    if (!importacion.Encabezados.Contains(nombreNuevaColumna))
                    {
                        importacion.Encabezados.Add(nombreNuevaColumna);
                    }

                    foreach (var fila in importacion.Filas)
                    {
                        if (fila.TryGetValue(columna, out var val) && val != null)
                        {
                            var textoLimpio = Regex.Replace(val.ToString(), @"[^\d.-]", "");
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
                // Ignorar si hay fallo de red, se continua con la carga sin enriquecer
            }
        }

        private static void EnriquecerMapas(DataArenaFusion.Models.TablaImportada importacion)
        {
            try
            {
                var palabrasClaveCiudad = new[] { "ciudad", "city", "ubicacion", "location" };
                var columnaCiudad = importacion.Encabezados
                    .FirstOrDefault(c => palabrasClaveCiudad.Any(pc => c.Contains(pc, StringComparison.OrdinalIgnoreCase)));

                if (columnaCiudad == null) return;

                if (!importacion.Encabezados.Contains("Latitud")) importacion.Encabezados.Add("Latitud");
                if (!importacion.Encabezados.Contains("Longitud")) importacion.Encabezados.Add("Longitud");

                var cache = new Dictionary<string, (string Lat, string Lon)>(StringComparer.OrdinalIgnoreCase);

                foreach (var fila in importacion.Filas)
                {
                    if (fila.TryGetValue(columnaCiudad, out var ciudadVal) && ciudadVal != null)
                    {
                        var ciudad = ciudadVal.ToString().Trim();
                        if (string.IsNullOrWhiteSpace(ciudad)) continue;

                        if (!cache.ContainsKey(ciudad))
                        {
                            // Llamada a Nominatim OpenStreetMap
                            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(ciudad)}&format=json&limit=1";
                            var response = _httpClient.GetAsync(url).Result;
                            
                            if (response.IsSuccessStatusCode)
                            {
                                var json = response.Content.ReadAsStringAsync().Result;
                                using var doc = JsonDocument.Parse(json);
                                if (doc.RootElement.GetArrayLength() > 0)
                                {
                                    var result = doc.RootElement[0];
                                    var lat = result.GetProperty("lat").GetString() ?? "";
                                    var lon = result.GetProperty("lon").GetString() ?? "";
                                    cache[ciudad] = (lat, lon);
                                }
                                else
                                {
                                    cache[ciudad] = ("", "");
                                }
                            }
                            else
                            {
                                cache[ciudad] = ("", ""); // Fallback
                            }
                            
                            // Nominatim Rate Limiting: 1 request per second max, sleep for unique searches
                            Thread.Sleep(1000); 
                        }

                        var coords = cache[ciudad];
                        fila["Latitud"] = coords.Lat;
                        fila["Longitud"] = coords.Lon;
                    }
                }
            }
            catch
            {
                // Ignorar si hay fallo de red, se continua con la carga sin enriquecer mapas
            }
        }
    }
}
