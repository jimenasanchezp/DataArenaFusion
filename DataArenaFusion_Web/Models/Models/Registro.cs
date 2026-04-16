using System.Collections.Generic;

namespace DataArenaFusion.Models
{
    public class Registro
    {
        public int Id { get; set; }
        public string Categoria { get; set; }
        public double Valor { get; set; }

        // Atrapa cualquier columna extra que no sean las 3 principales
        public Dictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }
}