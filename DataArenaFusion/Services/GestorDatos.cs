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
            TablaActual = new DataTable();
            RegistrosActuales = new List<Registro>();
            IndiceId = new Dictionary<int, Registro>();
        }

        public void CargarArchivo(string ruta)
        {
            var lector = LectorFactory.ObtenerLector(ruta);
            var importacion = lector.Leer(ruta);

            TablaActual = importacion.ToDataTable();
            RegistrosActuales = ConvertirARegistros(importacion);

            IndiceId.Clear();
            foreach (var reg in RegistrosActuales)
            {
                if (!IndiceId.ContainsKey(reg.Id))
                {
                    IndiceId.Add(reg.Id, reg);
                }
            }
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

        private static List<Registro> ConvertirARegistros(TablaImportada importacion)
        {
            var registros = new List<Registro>();
            var tieneId = importacion.Encabezados.Any(EncabezadoId);
            var tieneCategoria = importacion.Encabezados.Any(EncabezadoCategoria);
            var tieneValor = importacion.Encabezados.Any(EncabezadoValor);

            if (!tieneId || !tieneCategoria || !tieneValor)
            {
                return registros;
            }

            foreach (var fila in importacion.Filas)
            {
                var valorId = ObtenerValor(fila, "Id");
                var valorCategoria = ObtenerValor(fila, "Categoria");
                var valorImporte = ObtenerValor(fila, "Valor");

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
                foreach (var par in fila)
                {
                    if (!EncabezadoId(par.Key) && !EncabezadoCategoria(par.Key) && !EncabezadoValor(par.Key))
                    {
                        extras[par.Key] = par.Value;
                    }
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

        private static string ObtenerValor(IDictionary<string, string> fila, string encabezado)
        {
            foreach (var par in fila)
            {
                if (string.Equals(par.Key, encabezado, StringComparison.OrdinalIgnoreCase))
                {
                    return par.Value ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static bool EncabezadoId(string encabezado) =>
            string.Equals(encabezado, "Id", StringComparison.OrdinalIgnoreCase);

        private static bool EncabezadoCategoria(string encabezado) =>
            string.Equals(encabezado, "Categoria", StringComparison.OrdinalIgnoreCase);

        private static bool EncabezadoValor(string encabezado) =>
            string.Equals(encabezado, "Valor", StringComparison.OrdinalIgnoreCase);
    }
}
