using System.Collections.Generic;
using DataArenaFusion.Core.Models;
using System.Globalization;

namespace DataArenaFusion.Core.Processing.Procesadores
{
    public class ProcesadorDuplicados : IProcesadorDatos<List<Registro>>
    {
        private string _colId, _colCat, _colVal;

        public ProcesadorDuplicados(string colId, string colCat, string colVal)
        {
            _colId = colId;
            _colCat = colCat;
            _colVal = colVal;
        }

        public List<Registro> Procesar(List<Registro> datos, string columnaObjetivo)
        {
            var duplicados = new List<Registro>();
            var vistos = new HashSet<string>();
            if (datos == null) return duplicados;

            foreach (var r in datos)
            {
                string key = ObtenerValorColumna(r, columnaObjetivo);
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!vistos.Add(key))
                {
                    duplicados.Add(r);
                }
            }

            return duplicados;
        }

        private string ObtenerValorColumna(Registro r, string col)
        {
            if (col == _colId) return r.Id.ToString();
            if (col == _colCat) return r.Categoria;
            if (col == _colVal) return r.Valor.ToString(CultureInfo.InvariantCulture);
            
            if (r.Extras.TryGetValue(col, out var val)) return val;
            return "";
        }
    }
}
