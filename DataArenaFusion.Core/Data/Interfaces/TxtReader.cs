using DataArenaFusion.Core.Data.Interfaces;
using DataArenaFusion.Core.Models;

namespace DataArenaFusion.Core.Data
{
    public class TxtReader : ILectorArchivos
    {
        public TablaImportada Leer(string ruta)
        {
            return ImportadorTabular.LeerTxt(ruta);
        }
    }
}
