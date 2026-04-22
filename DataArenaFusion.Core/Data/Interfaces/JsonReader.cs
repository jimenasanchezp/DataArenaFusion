using DataArenaFusion.Core.Data.Interfaces;
using DataArenaFusion.Core.Models;

namespace DataArenaFusion.Core.Data
{
    public class JsonReader : ILectorArchivos
    {
        public TablaImportada Leer(string ruta)
        {
            return ImportadorTabular.LeerJson(ruta);
        }
    }
}
