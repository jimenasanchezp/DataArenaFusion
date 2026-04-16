using DataArenaFusion.Data.Interfaces;
using DataArenaFusion.Models;

namespace DataArenaFusion.Data
{
    public class CsvReader : ILectorArchivos
    {
        public TablaImportada Leer(string ruta)
        {
            return ImportadorTabular.LeerCsv(ruta);
        }
    }
}
