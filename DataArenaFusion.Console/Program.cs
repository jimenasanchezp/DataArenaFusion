using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DataArenaFusion.Core.Models;
using DataArenaFusion.Core.Services;

namespace DataArenaFusion.ConsoleApp
{
    class Program
    {
        private static readonly GestorDatos _gestor = new GestorDatos();

        static void Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("       DATA ARENA FUSION - SYSTEM CONSOLE         ");
            Console.WriteLine("==================================================");

            bool exit = false;
            while (!exit)
            {
                ShowMenu();
                string choice = Console.ReadLine() ?? "";
                exit = ProcessChoice(choice);
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("\n--- MENU PRINCIPAL ---");
            Console.WriteLine("1. Cargar archivo (CSV, JSON, XML, TXT)");
            Console.WriteLine("2. Ver previsualización de datos (Tabla)");
            Console.WriteLine("3. Ver resumen y Gráfica ASCII");
            Console.WriteLine("4. Contar duplicados");
            Console.WriteLine("5. Limpiar datos");
            Console.WriteLine("0. Salir");
            Console.Write("\nSeleccione una opción: ");
        }

        static bool ProcessChoice(string choice)
        {
            switch (choice)
            {
                case "1":
                    ImportFile();
                    break;
                case "2":
                    ShowDataPreview();
                    break;
                case "3":
                    ShowSummaryAndChart();
                    break;
                case "4":
                    CountDuplicates();
                    break;
                case "5":
                    _gestor.Limpiar();
                    Console.WriteLine("\nDatos limpiados con éxito.");
                    break;
                case "0":
                    return true;
                default:
                    Console.WriteLine("\nOpción no válida.");
                    break;
            }
            return false;
        }

        static void ImportFile()
        {
            Console.Write("\nIngrese la ruta del archivo (o presione Enter para buscar en TestData): ");
            string path = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // Intentar encontrar la carpeta TestData en la estructura del repo
                string sampleDataPath = Path.Combine(baseDir, "..", "..", "..", "..", "DataArenaFusion", "TestData");
                
                if (Directory.Exists(sampleDataPath))
                {
                    var files = Directory.GetFiles(sampleDataPath);
                    Console.WriteLine("\nArchivos encontrados en TestData:");
                    for (int i = 0; i < files.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
                    }
                    Console.Write("\nSeleccione un número o ingrese ruta completa: ");
                    string selection = Console.ReadLine() ?? "";
                    if (int.TryParse(selection, out int index) && index > 0 && index <= files.Length)
                    {
                        path = files[index - 1];
                    }
                    else
                    {
                        path = selection;
                    }
                }
            }

            if (File.Exists(path))
            {
                try
                {
                    Console.WriteLine($"Cargando {Path.GetFileName(path)}...");
                    _gestor.CargarArchivo(path);
                    Console.WriteLine($"¡Éxito! Se cargaron {_gestor.TablaActual.Rows.Count} registros.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar archivo: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("\nEl archivo no existe.");
            }
        }

        static void ShowDataPreview()
        {
            if (_gestor.TablaActual.Rows.Count == 0)
            {
                Console.WriteLine("\nNo hay datos cargados.");
                return;
            }

            Console.WriteLine("\n--- PREVISUALIZACIÓN DE DATOS (Top 20) ---");
            
            var columns = _gestor.TablaActual.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            
            // Print Headers
            string headerRow = string.Join(" | ", columns.Select(c => c.PadRight(15).Substring(0, 15)));
            Console.WriteLine(headerRow);
            Console.WriteLine(new string('-', headerRow.Length));

            // Print Rows
            int limit = Math.Min(20, _gestor.TablaActual.Rows.Count);
            for (int i = 0; i < limit; i++)
            {
                var row = _gestor.TablaActual.Rows[i];
                var cells = columns.Select(c => (row[c]?.ToString() ?? "").PadRight(15).Substring(0, 15));
                Console.WriteLine(string.Join(" | ", cells));
            }

            if (_gestor.TablaActual.Rows.Count > limit)
            {
                Console.WriteLine($"... y {_gestor.TablaActual.Rows.Count - limit} registros más.");
            }
        }

        static void ShowSummaryAndChart()
        {
            if (_gestor.RegistrosActuales.Count == 0)
            {
                Console.WriteLine("\nNo hay datos cargados.");
                return;
            }

            var summary = _gestor.ObtenerDatosParaGrafica();
            if (summary.Count == 0)
            {
                Console.WriteLine("\nNo se pudieron generar datos de resumen.");
                return;
            }

            Console.WriteLine("\n--- RESUMEN POR CATEGORÍA ---");
            foreach (var item in summary)
            {
                Console.WriteLine($"{item.Key.PadRight(20)}: {item.Value:N2}");
            }

            Console.WriteLine("\n--- GRÁFICA ASCII (VALOR TOTAL) ---");
            double maxValue = summary.Values.Max();
            int maxBarWidth = 40;

            foreach (var item in summary)
            {
                string label = item.Key.Length > 15 ? item.Key.Substring(0, 12) + "..." : item.Key.PadRight(15);
                int barLength = maxValue > 0 ? (int)((item.Value / maxValue) * maxBarWidth) : 0;
                string bar = new string('█', barLength);
                Console.WriteLine($"{label} | {bar} ({item.Value:N2})");
            }
        }

        static void CountDuplicates()
        {
            if (_gestor.RegistrosActuales.Count == 0)
            {
                Console.WriteLine("\nNo hay datos cargados.");
                return;
            }

            int count = _gestor.ContarDuplicados();
            Console.WriteLine($"\nSe encontraron {count} registros duplicados basados en el ID.");
        }
    }
}
