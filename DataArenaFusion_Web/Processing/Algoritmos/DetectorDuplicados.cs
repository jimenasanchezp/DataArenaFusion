using System.Collections.Generic;
using DataArenaFusion.Models;   
using DataArenaFusion.Processing;
using DataArenaFusion.Processing.Algoritmos;

namespace DataArenaFusion.Processing.Algoritmos
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