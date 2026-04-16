using DataArenaFusion.Data.Interfaces;
using DataArenaFusion.Models;

namespace DataArenaFusion.Data
{
    public class TxtReader : ILectorArchivos
    {
        public TablaImportada Leer(string ruta)
        {
            return ImportadorTabular.LeerTxt(ruta);
        }
    }
}
