using System.Globalization;
using System.Text;
using System.Xml.Linq;
using DataArenaFusion.Models;
using Newtonsoft.Json.Linq;

namespace DataArenaFusion.Data
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
            var texto = File.ReadAllText(ruta);
            var token = JToken.Parse(texto);

            IEnumerable<JToken> filas = token switch
            {
                JArray arreglo => arreglo,
                JObject objeto => new[] { objeto },
                _ => Array.Empty<JToken>()
            };

            var encabezados = new List<string>();

            foreach (var fila in filas.OfType<JObject>())
            {
                foreach (var propiedad in fila.Properties())
                {
                    if (!encabezados.Contains(propiedad.Name))
                    {
                        encabezados.Add(propiedad.Name);
                    }
                }
            }

            importacion.Encabezados.AddRange(encabezados);

            foreach (var fila in filas.OfType<JObject>())
            {
                var registro = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var encabezado in encabezados)
                {
                    var valor = fila.Property(encabezado)?.Value;
                    registro[encabezado] = ConvertirTokenAString(valor);
                }

                importacion.Filas.Add(registro);
            }

            return importacion;
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

            using var reader = new StreamReader(ruta);
            var primeraLinea = reader.ReadLine();

            if (primeraLinea == null)
            {
                return importacion;
            }

            var delimitador = DetectarDelimitador(primeraLinea, delimitadorPreferido);
            var encabezados = ParseLine(primeraLinea, delimitador);
            importacion.Encabezados.AddRange(encabezados);

            string? linea;
            while ((linea = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                var valores = ParseLine(linea, delimitador);
                var registro = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
            var candidatos = new[] { delimitadorPreferido, ',', '\t', ';', '|' }
                .Distinct()
                .ToList();

            return candidatos
                .OrderByDescending(c => linea.Count(ch => ch == c))
                .ThenBy(c => c != delimitadorPreferido)
                .First();
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
