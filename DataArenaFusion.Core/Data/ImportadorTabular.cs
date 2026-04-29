using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DataArenaFusion.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataArenaFusion.Core.Data
{
    public static class ImportadorTabular
    {
        public static TablaImportada LeerCsv(string ruta)
        {
            return LeerDelimitado(ruta, ',');
        }

        public static TablaImportada LeerTxt(string ruta)
        {
            return LeerDelimitado(ruta, '\t');
        }

        public static TablaImportada LeerJson(string ruta)
        {
            var importacion = new TablaImportada();
            using var stream = File.OpenRead(ruta);
            using var sr = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: false);
            using var reader = new JsonTextReader(sr);

            if (!reader.Read())
            {
                return importacion;
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        var objeto = JObject.Load(reader);
                        RegistrarJsonObjeto(importacion, objeto);
                    }
                    else if (reader.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                var objeto = JObject.Load(reader);
                RegistrarJsonObjeto(importacion, objeto);
            }

            return importacion;
        }

        private static void RegistrarJsonObjeto(TablaImportada importacion, JObject fila)
        {
            foreach (var propiedad in fila.Properties())
            {
                if (!importacion.Encabezados.Contains(propiedad.Name))
                {
                    importacion.Encabezados.Add(propiedad.Name);
                }
            }

            var registro = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var encabezado in importacion.Encabezados)
            {
                var valor = fila.Property(encabezado)?.Value;
                registro[encabezado] = ConvertirTokenAString(valor);
            }

            importacion.Filas.Add(registro);
        }

        public static TablaImportada LeerXml(string ruta)
        {
            var importacion = new TablaImportada();
            var documento = XDocument.Load(ruta);
            var raiz = documento.Root;

            if (raiz is null)
            {
                return importacion;
            }

            var filas = ObtenerNodosFila(raiz).ToList();
            var encabezados = new List<string>();

            foreach (var fila in filas)
            {
                foreach (var atributo in fila.Attributes())
                {
                    var nombre = $"@{atributo.Name.LocalName}";
                    if (!encabezados.Contains(nombre))
                    {
                        encabezados.Add(nombre);
                    }
                }

                foreach (var hijo in fila.Elements())
                {
                    if (!encabezados.Contains(hijo.Name.LocalName))
                    {
                        encabezados.Add(hijo.Name.LocalName);
                    }
                }
            }

            importacion.Encabezados.AddRange(encabezados);

            foreach (var fila in filas)
            {
                var registro = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var encabezado in encabezados)
                {
                    if (encabezado.StartsWith("@", StringComparison.Ordinal))
                    {
                        var nombreAtributo = encabezado[1..];
                        registro[encabezado] = fila.Attribute(nombreAtributo)?.Value ?? string.Empty;
                    }
                    else
                    {
                        registro[encabezado] = fila.Element(encabezado)?.Value ?? string.Empty;
                    }
                }

                importacion.Filas.Add(registro);
            }

            return importacion;
        }

        private static TablaImportada LeerDelimitado(string ruta, char delimitadorPreferido)
        {
            var importacion = new TablaImportada();

            using var reader = new StreamReader(ruta, Encoding.UTF8);
            var primeraLinea = reader.ReadLine();

            if (primeraLinea == null)
            {
                return importacion;
            }

            var delimitador = DetectarDelimitador(primeraLinea, delimitadorPreferido);
            var encabezados = ParseLine(primeraLinea, delimitador);

            // REPARACIÓN CRÍTICA: Si el delimitador detectado nos da solo 1 columna 
            // pero la línea tiene espacios, es probable que el separador sea el espacio.
            if (encabezados.Count <= 1 && primeraLinea.Contains(' '))
            {
                // Intentamos separar por cualquier cantidad de espacios en blanco
                var intentoEspacios = primeraLinea.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (intentoEspacios.Length > 1)
                {
                    delimitador = ' '; // Forzamos modo espacio
                    encabezados = intentoEspacios.Select(s => s.Trim()).ToList();
                }
            }

            importacion.Encabezados.AddRange(encabezados);

            Console.WriteLine($"[CSV/TXT] Delimitador final: '{(delimitador == '\t' ? "\\t" : delimitador == ' ' ? "Espacio" : delimitador.ToString())}'. Columnas: {encabezados.Count}");

            string? linea;
            while ((linea = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                List<string> valores;
                if (delimitador == ' ')
                {
                    // Si el delimitador es espacio, usamos Split con RemoveEmptyEntries para manejar múltiples espacios
                    valores = linea.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToList();
                }
                else
                {
                    valores = ParseLine(linea, delimitador);
                }

                var registro = new Dictionary<string, string>(encabezados.Count, StringComparer.OrdinalIgnoreCase);

                for (int j = 0; j < encabezados.Count; j++)
                {
                    var valor = j < valores.Count ? valores[j] : string.Empty;
                    registro[encabezados[j]] = valor;
                }

                importacion.Filas.Add(registro);
            }

            return importacion;
        }

        private static List<XElement> ObtenerNodosFila(XElement raiz)
        {
            var hijosDirectos = raiz.Elements().ToList();
            if (hijosDirectos.Count == 0)
            {
                return new List<XElement>();
            }

            var grupoMasComun = hijosDirectos
                .GroupBy(x => x.Name.LocalName)
                .OrderByDescending(g => g.Count())
                .First();

            if (grupoMasComun.Count() > 1)
            {
                return grupoMasComun.ToList();
            }

            if (hijosDirectos.Count == 1 && hijosDirectos[0].Elements().Any())
            {
                return hijosDirectos[0].Elements().ToList();
            }

            return hijosDirectos;
        }

        private static char DetectarDelimitador(string linea, char delimitadorPreferido)
        {
            // Candidatos: Tabulador, Coma, Punto y Coma, Pipe
            var candidatos = new[] { delimitadorPreferido, '\t', ',', ';', '|' }
                .Distinct()
                .ToList();

            // Contar ocurrencias
            var recuento = candidatos.Select(c => new { Char = c, Count = linea.Count(ch => ch == c) })
                                    .OrderByDescending(x => x.Count)
                                    .ToList();

            // Si el mejor candidato tiene 0 apariciones, devolvemos el preferido
            if (recuento[0].Count == 0) return delimitadorPreferido;

            return recuento[0].Char;
        }

        private static List<string> ParseLine(string linea, char delimitador)
        {
            var valores = new List<string>();
            var actual = new StringBuilder();
            var entreComillas = false;

            for (int i = 0; i < linea.Length; i++)
            {
                var caracter = linea[i];

                if (caracter == '\"')
                {
                    if (entreComillas && i + 1 < linea.Length && linea[i + 1] == '\"')
                    {
                        actual.Append('\"');
                        i++;
                    }
                    else
                    {
                        entreComillas = !entreComillas;
                    }
                }
                else if (caracter == delimitador && !entreComillas)
                {
                    valores.Add(actual.ToString().Trim());
                    actual.Clear();
                }
                else
                {
                    actual.Append(caracter);
                }
            }

            valores.Add(actual.ToString().Trim());
            return valores;
        }

        private static string ConvertirTokenAString(JToken? token)
        {
            return token switch
            {
                null => string.Empty,
                JValue valor => Convert.ToString(valor.Value, CultureInfo.InvariantCulture) ?? string.Empty,
                _ => token.ToString()
            };
        }
    }
}
