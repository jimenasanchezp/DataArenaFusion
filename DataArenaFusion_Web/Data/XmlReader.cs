using DataArenaFusion.Data.Interfaces;
using DataArenaFusion.Models;

namespace DataArenaFusion.Data
{
    // Le cambiamos el nombre para que no choque con el XmlReader nativo de C#
    public class XmlReader : ILectorArchivos
    {
        public TablaImportada Leer(string ruta)
        {
            return ImportadorTabular.LeerXml(ruta);
        }
    }
}
