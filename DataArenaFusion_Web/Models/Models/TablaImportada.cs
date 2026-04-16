using System.Data;

namespace DataArenaFusion.Models
{
    public class TablaImportada
    {
        public List<string> Encabezados { get; } = new();
        public List<Dictionary<string, string>> Filas { get; } = new();

        public DataTable ToDataTable()
        {
            var tabla = new DataTable();
            var nombresColumnas = new List<string>();

            foreach (var encabezado in Encabezados)
            {
                var nombreUnico = ObtenerNombreUnico(nombresColumnas, encabezado);
                nombresColumnas.Add(nombreUnico);
                tabla.Columns.Add(nombreUnico, typeof(string));
            }

            if (tabla.Columns.Count == 0 && Filas.Count > 0)
            {
                foreach (var encabezado in Filas[0].Keys)
                {
                    var nombreUnico = ObtenerNombreUnico(nombresColumnas, encabezado);
                    nombresColumnas.Add(nombreUnico);
                    tabla.Columns.Add(nombreUnico, typeof(string));
                }
            }

            foreach (var fila in Filas)
            {
                var row = tabla.NewRow();

                for (int i = 0; i < nombresColumnas.Count; i++)
                {
                    var nombreColumna = nombresColumnas[i];
                    row[nombreColumna] = fila.TryGetValue(nombreColumna, out var valor)
                        ? valor ?? string.Empty
                        : string.Empty;
                }

                tabla.Rows.Add(row);
            }

            return tabla;
        }

        private static string ObtenerNombreUnico(ICollection<string> existentes, string baseName)
        {
            if (!existentes.Contains(baseName))
            {
                return baseName;
            }

            int sufijo = 2;
            string candidato;
            do
            {
                candidato = $"{baseName}_{sufijo}";
                sufijo++;
            } while (existentes.Contains(candidato));

            return candidato;
        }
    }
}
