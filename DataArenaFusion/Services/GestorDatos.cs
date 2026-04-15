using System.Collections.Generic;
using DataArenaFusion.Models;
using DataArenaFusion.Data;
using DataArenaFusion.Processing.Algoritmos;

namespace DataArenaFusion.Services
{
    public class GestorDatos
    {
        public List<Registro> RegistrosActuales { get; private set; }
        public Dictionary<int, Registro> IndiceId { get; private set; }

        public GestorDatos()
        {
            RegistrosActuales = new List<Registro>();
            IndiceId = new Dictionary<int, Registro>();
        }

        public void CargarArchivo(string ruta)
        {
            // 1. Usa la Factory para obtener el lector correcto
            var lector = LectorFactory.ObtenerLector(ruta);

            // 2. Lee los datos
            RegistrosActuales = lector.Leer(ruta);

            // 3. Reconstruye el índice de diccionario para búsquedas rápidas
            IndiceId.Clear();
            foreach (var reg in RegistrosActuales)
            {
                // Protege contra IDs duplicados en la llave
                if (!IndiceId.ContainsKey(reg.Id))
                    IndiceId.Add(reg.Id, reg);
            }
        }

        // --- Wrappers para los Algoritmos ---

        public void OrdenarAscendente()
        {
            OrdenadorDatos.BubbleSortPorValor(RegistrosActuales, true);
        }

        public Dictionary<string, double> ObtenerDatosParaGrafica()
        {
            return Agrupador.SumarPorCategoria(RegistrosActuales);
        }

        public int ContarDuplicados()
        {
            return DetectorDuplicados.ObtenerDuplicados(RegistrosActuales).Count;
        }
    }
}