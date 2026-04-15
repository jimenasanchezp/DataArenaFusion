using System.Collections.Generic;
using DataArenaFusion.Models;
using DataArenaFusion.Processing;
using DataArenaFusion.Processing.Algoritmos;


namespace DataArenaFusion.Processing.Algoritmos
{
    public static class OrdenadorDatos
    {
        public static void BubbleSortPorValor(List<Registro> datos, bool ascendente = true)
        {
            int n = datos.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    bool condicion = ascendente
                        ? datos[j].Valor > datos[j + 1].Valor
                        : datos[j].Valor < datos[j + 1].Valor;

                    if (condicion)
                    {
                        // Intercambio sin LINQ
                        Registro temp = datos[j];
                        datos[j] = datos[j + 1];
                        datos[j + 1] = temp;
                    }
                }
            }
        }
    }
}