using System.Collections.Generic;
using DataArenaFusion.Core.Models;
using DataArenaFusion.Core.Processing;
using DataArenaFusion.Core.Processing.Algoritmos;

namespace DataArenaFusion.Core.Processing.Algoritmos
{
    public static class Agrupador
    {
        public static Dictionary<string, double> SumarPorCategoria(List<Registro> datos)
        {
            var agrupado = new Dictionary<string, double>();

            for (int i = 0; i < datos.Count; i++)
            {
                string cat = datos[i].Categoria;
                double val = datos[i].Valor;

                if (agrupado.ContainsKey(cat))
                    agrupado[cat] += val;
                else
                    agrupado.Add(cat, val);
            }
            return agrupado;
        }
    }
}