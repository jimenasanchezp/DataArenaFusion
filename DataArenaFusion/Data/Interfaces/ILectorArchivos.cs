using System.Collections.Generic;
using DataArenaFusion.Models;

namespace DataArenaFusion.Data.Interfaces
{
    public interface ILectorArchivos
    {
        List<Registro> Leer(string ruta);
    }
}