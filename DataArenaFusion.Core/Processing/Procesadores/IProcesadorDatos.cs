using System.Collections.Generic;
using DataArenaFusion.Core.Models;

namespace DataArenaFusion.Core.Processing.Procesadores
{
    public interface IProcesadorDatos<TResult>
    {
        TResult Procesar(List<Registro> datos, string columnaObjetivo);
    }
}
