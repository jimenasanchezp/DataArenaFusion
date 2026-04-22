using System.Collections.Generic;
using DataArenaFusion.Core.Models;   
using DataArenaFusion.Core.Processing;
using DataArenaFusion.Core.Processing.Algoritmos;

namespace DataArenaFusion.Core.Processing.Algoritmos
{
    public static class DetectorDuplicados
    {
        public static List<Registro> ObtenerDuplicados(List<Registro> datos)
        {
            var duplicados = new List<Registro>();
            var idsVistos = new HashSet<int>();

            for (int i = 0; i < datos.Count; i++)
            {
                if (!idsVistos.Add(datos[i].Id))
                {
                    duplicados.Add(datos[i]);
                }
            }
            return duplicados;
        }
    }
}