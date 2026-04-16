using System.Collections.Generic;
using DataArenaFusion.Models;
using System.Globalization;

namespace DataArenaFusion.Processing.Procesadores
{
    public class ProcesadorOrdenamiento : IProcesadorDatos<List<Registro>>
    {
        private string _colId, _colCat, _colVal;

        public ProcesadorOrdenamiento(string colId, string colCat, string colVal)
        {
            _colId = colId;
            _colCat = colCat;
            _colVal = colVal;
        }

        public List<Registro> Procesar(List<Registro> datos, string columnaObjetivo)
        {
            if (datos == null || datos.Count <= 1) return datos;
            
            QuickSort(datos, 0, datos.Count - 1, columnaObjetivo);
            return datos;
        }

        private void QuickSort(List<Registro> list, int low, int high, string col)
        {
            if (low < high)
            {
                int pivotIndex = Partition(list, low, high, col);
                QuickSort(list, low, pivotIndex - 1, col);
                QuickSort(list, pivotIndex + 1, high, col);
            }
        }

        private int Partition(List<Registro> list, int low, int high, string col)
        {
            Registro pivot = list[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                if (Comparar(list[j], pivot, col) <= 0)
                {
                    i++;
                    Registro temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }

            Registro tempFinal = list[i + 1];
            list[i + 1] = list[high];
            list[high] = tempFinal;

            return i + 1;
        }

        private int Comparar(Registro a, Registro b, string col)
        {
            string valA = ObtenerValorColumna(a, col);
            string valB = ObtenerValorColumna(b, col);

            // Intentar comparacion numerica
            if (double.TryParse(valA, NumberStyles.Any, CultureInfo.InvariantCulture, out double numA) && 
                double.TryParse(valB, NumberStyles.Any, CultureInfo.InvariantCulture, out double numB))
            {
                return numA.CompareTo(numB);
            }

            // Comparacion alfabetica
            return string.Compare(valA, valB, System.StringComparison.OrdinalIgnoreCase);
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
