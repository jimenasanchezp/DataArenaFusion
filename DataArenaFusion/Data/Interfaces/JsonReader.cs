using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using DataArenaFusion.Models;
using DataArenaFusion.Data;
using DataArenaFusion.Data.Interfaces;

namespace DataArenaFusion.Data
{
    public class JsonReader : ILectorArchivos
    {
        public List<Registro> Leer(string ruta)
        {
            string json = File.ReadAllText(ruta);
            return JsonConvert.DeserializeObject<List<Registro>>(json) ?? new List<Registro>();
        }
    }
}