namespace DataArenaFusion.Models;
public class DataArena
{
    /// <summary>
    /// Lista principal de registros cargados
    /// </summary>
    public List<Registro> Registros { get; set; }

    /// <summary>
    /// Nombres de todas las columnas (en orden)
    /// </summary>
    public List<string> NombreColumnas { get; set; }

    /// <summary>
    /// Tipo de archivo original
    /// </summary>
    public string TipoArchivo { get; set; }

    /// <summary>
    /// Nombre del archivo
    /// </summary>
    public string NombreArchivo { get; set; }

    public DataArena()
    {
        Registros = new List<Registro>();
        NombreColumnas = new List<string>();
        TipoArchivo = string.Empty;
        NombreArchivo = string.Empty;
    }
}