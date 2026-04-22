using DataArenaFusion.Core.Models;

namespace DataArenaFusion.Core.Data.Interfaces
{
    public interface ILectorArchivos
    {
        TablaImportada Leer(string ruta);
    }
}
