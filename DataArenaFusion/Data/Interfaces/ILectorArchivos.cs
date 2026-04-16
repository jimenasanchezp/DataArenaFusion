using DataArenaFusion.Models;

namespace DataArenaFusion.Data.Interfaces
{
    public interface ILectorArchivos
    {
        TablaImportada Leer(string ruta);
    }
}
