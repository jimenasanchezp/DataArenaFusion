using System;
using DataArenaFusion.Models;
namespace DataArenaFusion.Models;
public class Registro
{
    /// <summary>
    /// Almacena los valores de cada columna para este registro
    /// Clave: nombre de columna, Valor: el dato
    /// </summary>
    public Dictionary<string, object> Datos { get; set; }

    public Registro()
    {
        Datos = new Dictionary<string, object>();
    }
}