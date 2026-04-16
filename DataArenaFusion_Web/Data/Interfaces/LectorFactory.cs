using DataArenaFusion.Data.Interfaces;
using System;
using System.Formats.Asn1;
using System.Formats.Tar;
using System.IO;
using System.Xml;

namespace DataArenaFusion.Data
{
    public static class LectorFactory
    {
        public static ILectorArchivos ObtenerLector(string ruta)
        {
            string extension = Path.GetExtension(ruta).ToLower();

            return extension switch
            {
                ".csv" => new CsvReader(),
                ".txt" => new TxtReader(),
                ".json" => new JsonReader(),
                ".xml" => new XmlReader(),
                _ => throw new NotSupportedException($"Formato {extension} no soportado.")
            };
        }
    }
}