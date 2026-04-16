using System.Collections.Generic;
using System.Xml;
using DataArenaFusion.Models;
using DataArenaFusion.Data.Interfaces;

namespace DataArenaFusion.Data
{
    // Le cambiamos el nombre para que no choque con el XmlReader nativo de C#
    public class XmlReader : ILectorArchivos
    {
        public List<Registro> Leer(string ruta)
        {
            var lista = new List<Registro>();
            XmlDocument doc = new XmlDocument();
            doc.Load(ruta);

            // Esto asume que tu XML tiene etiquetas <Registro> con <Id>, <Categoria> y <Valor>
            foreach (XmlNode nodo in doc.SelectNodes("//Registro"))
            {
                lista.Add(new Registro
                {
                    Id = int.Parse(nodo["Id"].InnerText),
                    Categoria = nodo["Categoria"].InnerText,
                    Valor = double.Parse(nodo["Valor"].InnerText)
                });
            }
            return lista;
        }
    }
}