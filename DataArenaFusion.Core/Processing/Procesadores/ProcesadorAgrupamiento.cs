using System.Collections.Generic;
using DataArenaFusion.Core.Models;
using System.Globalization;

namespace DataArenaFusion.Core.Processing.Procesadores
{
    public class ProcesadorAgrupamiento : IProcesadorDatos<Dictionary<string, double>>
    {
        private string _colId, _colCat, _colVal;

        public ProcesadorAgrupamiento(string colId, string colCat, string colVal)
        {
            _colId = colId;
            _colCat = colCat;
            _colVal = colVal;
        }

        public Dictionary<string, double> Procesar(List<Registro> datos, string columnaObjetivo)
        {
            var agrupado = new Dictionary<string, double>();
            if (datos == null) return agrupado;

            bool isCounting = string.IsNullOrEmpty(_colVal);

            foreach (var r in datos)
            {
                string key = ObtenerValorColumna(r, columnaObjetivo);
                if (string.IsNullOrWhiteSpace(key)) key = "[Sin valor]";

                double increment = isCounting ? 1 : r.Valor;

                if (agrupado.ContainsKey(key))
                    agrupado[key] += increment;
                else
                    agrupado.Add(key, increment);
            }

            return agrupado;
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
