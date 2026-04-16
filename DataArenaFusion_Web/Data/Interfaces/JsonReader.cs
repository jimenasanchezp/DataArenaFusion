using DataArenaFusion.Data.Interfaces;
using DataArenaFusion.Models;

namespace DataArenaFusion.Data
{
    public class JsonReader : ILectorArchivos
    {
        public TablaImportada Leer(string ruta)
        {
            return ImportadorTabular.LeerJson(ruta);
        }
    }
}
