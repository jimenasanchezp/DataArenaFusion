using DataArenaFusion.Core.Data.Interfaces;
using DataArenaFusion.Core.Models;

namespace DataArenaFusion.Core.Data
{
    public class CsvReader : ILectorArchivos
    {
        public TablaImportada Leer(string ruta)
        {
            return ImportadorTabular.LeerCsv(ruta);
        }
    }
}
