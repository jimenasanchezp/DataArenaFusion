using System.Collections.Generic;
using DataArenaFusion.Data;

namespace DataArenaFusion.Data.Interfaces
{
    public interface ILectorArchivos
    {
        List<Registro> Leer(string ruta);
    }
}