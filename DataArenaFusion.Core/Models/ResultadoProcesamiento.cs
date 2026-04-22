namespace DataArenaFusion.Core.Models;
public class ResultadoProcesamiento
{
    /// <summary>
    /// Datos originales
    /// </summary>
    public DataArena DatosOriginales { get; set; }

    /// <summary>
    /// Datos después del procesamiento
    /// </summary>
    public DataArena DatosProcesados { get; set; }

    /// <summary>
    /// Información de qué se hizo
    /// </summary>
    public string Descripcion { get; set; }

    public ResultadoProcesamiento()
    {
        DatosOriginales = new DataArena();
        DatosProcesados = new DataArena();
        Descripcion = string.Empty;
    }
}