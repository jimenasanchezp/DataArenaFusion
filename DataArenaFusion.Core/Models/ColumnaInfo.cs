using DataArenaFusion.Core.Models;
public class ColumnaInfo
{
    /// <summary>
    /// Nombre de la columna
    /// </summary>
    public string Nombre { get; set; }

    /// <summary>
    /// Tipo de dato detectado (string, int, double, etc.)
    /// </summary>
    public Type TipoDato { get; set; }

    /// <summary>
    /// Índice de la columna
    /// </summary>
    public int Indice { get; set; }

    public ColumnaInfo(string nombre, Type tipoDato, int indice)
    {
        Nombre = nombre;
        TipoDato = tipoDato;
        Indice = indice;
    }
}