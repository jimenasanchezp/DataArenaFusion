using System.Collections.Generic;
using DataArenaFusion.Models;

namespace DataArenaFusion.Processing.Procesadores
{
    public interface IProcesadorDatos<TResult>
    {
        TResult Procesar(List<Registro> datos, string columnaObjetivo);
    }
}
