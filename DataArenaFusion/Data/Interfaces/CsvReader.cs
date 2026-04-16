using System.Collections.Generic;
using System.IO;
using DataArenaFusion.Models;   
using DataArenaFusion.Data;
using DataArenaFusion.Data.Interfaces;

namespace DataArenaFusion.Data
{
    public class CsvReader : ILectorArchivos
    {
        public List<Registro> Leer(string ruta)
        {
            var lista = new List<Registro>();
            string[] lineas = File.ReadAllLines(ruta);
            if (lineas.Length <= 1) return lista; // Vacío o solo encabezados

            for (int i = 1; i < lineas.Length; i++)
            {
                string[] col = lineas[i].Split(',');
                if (col.Length >= 3)
                {
                    lista.Add(new Registro
                    {
                        Id = int.Parse(col[0]),
                        Categoria = col[1],
                        Valor = double.Parse(col[2])
                    });
                }
            }
            return lista;
        }
    }
}