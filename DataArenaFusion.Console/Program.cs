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

        // Colores para la gráfica de barras
        private static readonly ConsoleColor[] BarColors = {
            ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Yellow,
            ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Blue,
            ConsoleColor.DarkCyan, ConsoleColor.DarkYellow
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            WriteColored("\n  ╔══════════════════════════════════════════════════╗\n", ConsoleColor.Cyan);
            WriteColored("  ║       DATA ARENA FUSION — SYSTEM CONSOLE        ║\n", ConsoleColor.Cyan);
            WriteColored("  ╚══════════════════════════════════════════════════╝\n", ConsoleColor.Cyan);

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
            Console.WriteLine();
            WriteColored("  ── MENÚ PRINCIPAL ──\n", ConsoleColor.DarkCyan);
            Console.WriteLine("  1. Cargar archivo (CSV, JSON, XML, TXT)");
            Console.WriteLine("  2. Ver previsualización de datos (Tabla)");
            Console.WriteLine("  3. Ver resumen y Gráfica de Barras ASCII");
            Console.WriteLine("  4. Contar duplicados");
            Console.WriteLine("  5. Filtrar datos (Búsqueda)");
            Console.WriteLine("  6. Limpiar datos");
            Console.WriteLine("  0. Salir");
            WriteColored("\n  Seleccione una opción: ", ConsoleColor.White);
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
                    FilterData();
                    break;
                case "6":
                    _gestor.Limpiar();
                    WriteColored("\n  ✓ Datos limpiados con éxito.\n", ConsoleColor.Green);
                    break;
                case "0":
                    return true;
                default:
                    WriteColored("\n  ✗ Opción no válida.\n", ConsoleColor.Red);
                    break;
            }
            return false;
        }

        static void ImportFile()
        {
            Console.Write("\n  Ingrese la ruta del archivo (o Enter para buscar en TestData): ");
            string path = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // Intentar encontrar la carpeta TestData en la estructura del repo
                string sampleDataPath = Path.Combine(baseDir, "..", "..", "..", "..", "DataArenaFusion", "TestData");

                if (Directory.Exists(sampleDataPath))
                {
                    var files = Directory.GetFiles(sampleDataPath);
                    WriteColored("\n  Archivos encontrados en TestData:\n", ConsoleColor.DarkCyan);
                    for (int i = 0; i < files.Length; i++)
                    {
                        Console.WriteLine($"    {i + 1}. {Path.GetFileName(files[i])}");
                    }
                    Console.Write("\n  Seleccione un número o ingrese ruta completa: ");
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
                    Console.WriteLine($"  Cargando {Path.GetFileName(path)}...");
                    _gestor.CargarArchivo(path);
                    WriteColored($"  ✓ ¡Éxito! Se cargaron {_gestor.TablaActual.Rows.Count} registros.\n", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    WriteColored($"  ✗ Error al cargar archivo: {ex.Message}\n", ConsoleColor.Red);
                }
            }
            else
            {
                WriteColored("\n  ✗ El archivo no existe.\n", ConsoleColor.Red);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  TABLA — Previsualización con bordes y ancho dinámico
        // ═══════════════════════════════════════════════════════
        static void ShowDataPreview()
        {
            if (_gestor.TablaActual.Rows.Count == 0)
            {
                WriteColored("\n  No hay datos cargados.\n", ConsoleColor.Yellow);
                return;
            }

            int limit = Math.Min(20, _gestor.TablaActual.Rows.Count);
            var columns = _gestor.TablaActual.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

            // Limitar a las primeras 8 columnas si son demasiadas para la consola
            int maxCols = Math.Min(columns.Count, 8);
            columns = columns.Take(maxCols).ToList();

            // Calcular ancho óptimo por columna (basado en encabezado + datos reales)
            var widths = CalculateColumnWidths(columns, limit, maxWidth: 22);

            // Título
            WriteColored($"\n  ── PREVISUALIZACIÓN DE DATOS (Top {limit} de {_gestor.TablaActual.Rows.Count}) ──\n\n", ConsoleColor.DarkCyan);

            // Línea superior
            Console.Write("  ┌");
            Console.WriteLine(string.Join("┬", columns.Select((c, i) => new string('─', widths[i] + 2))));

            // Encabezados
            Console.Write("  │");
            for (int c = 0; c < columns.Count; c++)
            {
                var hdr = TruncPad(columns[c], widths[c]);
                Console.Write(" ");
                WriteColored(hdr, ConsoleColor.Cyan);
                Console.Write(" │");
            }
            Console.WriteLine();

            // Separador encabezado-datos
            Console.Write("  ├");
            Console.WriteLine(string.Join("┼", columns.Select((_, i) => new string('─', widths[i] + 2))));

            // Filas de datos
            for (int r = 0; r < limit; r++)
            {
                var row = _gestor.TablaActual.Rows[r];
                Console.Write("  │");
                for (int c = 0; c < columns.Count; c++)
                {
                    var val = row[columns[c]]?.ToString() ?? "";
                    Console.Write($" {TruncPad(val, widths[c])} │");
                }
                Console.WriteLine();
            }

            // Línea inferior
            Console.Write("  └");
            Console.WriteLine(string.Join("┴", columns.Select((_, i) => new string('─', widths[i] + 2))));

            if (_gestor.TablaActual.Rows.Count > limit)
            {
                WriteColored($"  ... y {_gestor.TablaActual.Rows.Count - limit} registros más.", ConsoleColor.DarkGray);
                if (columns.Count < _gestor.TablaActual.Columns.Count)
                    WriteColored($"  ({_gestor.TablaActual.Columns.Count - maxCols} columnas ocultas)", ConsoleColor.DarkGray);
                Console.WriteLine();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  GRÁFICA DE BARRAS ASCII — con colores, degradados e iconos
        // ═══════════════════════════════════════════════════════
        static void ShowSummaryAndChart()
        {
            if (_gestor.RegistrosActuales.Count == 0)
            {
                WriteColored("\n  ⚠ No hay datos cargados.\n", ConsoleColor.Yellow);
                return;
            }

            var summary = _gestor.ObtenerDatosParaGrafica();
            if (summary.Count == 0)
            {
                WriteColored("\n  ⚠ No se pudieron generar datos de resumen.\n", ConsoleColor.Yellow);
                return;
            }

            // 1. CÁLCULO DE ANCHOS DINÁMICOS PARA ALINEACIÓN PERFECTA
            int maxKeyLen = Math.Max(12, summary.Keys.Max(k => k.Length));
            maxKeyLen = Math.Min(maxKeyLen, 30);
            int valueWidth = 16; // Ancho fijo para la columna numérica

            // 2. TABLA DE RESUMEN PREMIUM (Bordes Dobles)
            WriteColored("\n  📋 RESUMEN POR CATEGORÍA\n", ConsoleColor.Cyan);

            // Línea Superior: ╔══╦══╗
            Console.Write("  ╔" + new string('═', maxKeyLen + 2) + "╦" + new string('═', valueWidth + 2) + "╗\n");

            // Encabezado
            Console.Write("  ║ ");
            WriteColored("Categoría".PadRight(maxKeyLen), ConsoleColor.White);
            Console.Write(" ║ ");
            WriteColored("Valor Total".PadLeft(valueWidth), ConsoleColor.White);
            Console.Write(" ║\n");

            // Separador Central: ╠══╬══╣
            Console.Write("  ╠" + new string('═', maxKeyLen + 2) + "╬" + new string('═', valueWidth + 2) + "╣\n");

            // Filas de Datos
            foreach (var item in summary)
            {
                Console.Write("  ║ ");
                Console.Write(TruncPad(item.Key, maxKeyLen));
                Console.Write(" ║ ");
                WriteColored(item.Value.ToString("N2").PadLeft(valueWidth), ConsoleColor.Green);
                Console.Write(" ║\n");
            }

            // Línea Inferior: ╚══╩══╝
            Console.Write("  ╚" + new string('═', maxKeyLen + 2) + "╩" + new string('═', valueWidth + 2) + "╝\n");

            // 3. ANÁLISIS VISUAL (GRÁFICA)
            WriteColored("\n  📊 ANÁLISIS VISUAL (VALOR TOTAL)\n", ConsoleColor.Cyan);

            double maxValue = summary.Values.Max();

            // MANEJO DE CEROS: Si el máximo es 0, no hay nada que graficar
            if (maxValue <= 0)
            {
                WriteColored("     (No hay valores mayores a cero para generar barras visuales)\n", ConsoleColor.DarkGray);
                return;
            }

            int maxBarWidth = 40;
            int labelWidth = 15;
            int colorIdx = 0;

            foreach (var item in summary)
            {
                string label = TruncPad(item.Key, labelWidth);
                
                // Cálculo proporcional
                double ratio = item.Value / maxValue;
                int barLen = (int)(ratio * maxBarWidth);
                
                // DEGRADADO VISUAL: Usar ▓ para representar fracciones de bloque
                string bar = new string('█', barLen);
                if (barLen < maxBarWidth && (ratio * maxBarWidth - barLen) > 0.4)
                    bar += "▓";

                var color = BarColors[colorIdx % BarColors.Length];
                colorIdx++;

                Console.Write($"  {label} │ ");
                WriteColored(bar.PadRight(maxBarWidth), color);
                WriteColored($" {item.Value:N2}\n", ConsoleColor.DarkGray);
            }

            // Eje X y Escala
            Console.Write($"  {new string(' ', labelWidth)} └" + new string('─', maxBarWidth) + ">\n");
            
            // Escala simplificada
            string vMin = "0";
            string vMid = (maxValue / 2).ToString("N0");
            string vMax = maxValue.ToString("N0");
            
            string scale = $"  {new string(' ', labelWidth)}  {vMin}{vMid.PadLeft(maxBarWidth/2)}{vMax.PadLeft(maxBarWidth/2)}";
            WriteColored(scale + "\n", ConsoleColor.DarkGray);

            Console.WriteLine($"\n  Total categorías: {summary.Count}  |  Valor máximo detectado: {maxValue:N2}");
        }

        static void CountDuplicates()
        {
            if (_gestor.RegistrosActuales.Count == 0)
            {
                WriteColored("\n  ⚠ No hay datos cargados.\n", ConsoleColor.Yellow);
                return;
            }

            int count = _gestor.ContarDuplicados();
            Console.WriteLine($"\n  Se encontraron {count} registros duplicados basados en el ID.");
        }

        static void FilterData()
        {
            if (_gestor.RegistrosActuales.Count == 0)
            {
                WriteColored("\n  ⚠ No hay datos cargados.\n", ConsoleColor.Yellow);
                return;
            }

            Console.Write("\n  Ingrese el término de búsqueda: ");
            string term = Console.ReadLine() ?? "";

            var results = _gestor.Filtrar(term);
            Console.WriteLine($"\n  Se encontraron {results.Count} resultados:");

            if (results.Count > 0)
            {
                // Construir tabla de resultados con las 3 columnas principales
                var cols = new List<string> { "Id", "Categoría", "Valor" };
                int limit = Math.Min(10, results.Count);

                // Calcular anchos
                int wId = Math.Max(4, results.Take(limit).Any() ? results.Take(limit).Max(r => r.Id.ToString().Length) : 4);
                int wCat = Math.Max(10, results.Take(limit).Any() ? results.Take(limit).Max(r => r.Categoria.Length) : 10);
                wCat = Math.Min(wCat, 25);
                int wVal = Math.Max(8, results.Take(limit).Any() ? results.Take(limit).Max(r => r.Valor.ToString("N2").Length) : 8);
                var widths = new[] { wId, wCat, wVal };

                Console.Write("\n  ┌");
                Console.WriteLine(string.Join("┬", widths.Select(w => new string('─', w + 2))));

                Console.Write("  │");
                for (int c = 0; c < cols.Count; c++)
                {
                    Console.Write(" ");
                    WriteColored(TruncPad(cols[c], widths[c]), ConsoleColor.Cyan);
                    Console.Write(" │");
                }
                Console.WriteLine();

                Console.Write("  ├");
                Console.WriteLine(string.Join("┼", widths.Select(w => new string('─', w + 2))));

                for (int i = 0; i < limit; i++)
                {
                    var r = results[i];
                    Console.Write("  │");
                    Console.Write($" {TruncPad(r.Id.ToString(), widths[0])} │");
                    Console.Write($" {TruncPad(r.Categoria, widths[1])} │");
                    Console.Write($" {r.Valor.ToString("N2").PadLeft(widths[2])} │");
                    Console.WriteLine();
                }

                Console.Write("  └");
                Console.WriteLine(string.Join("┴", widths.Select(w => new string('─', w + 2))));

                if (results.Count > limit)
                    WriteColored($"  ... y {results.Count - limit} más.\n", ConsoleColor.DarkGray);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Utilidades
        // ═══════════════════════════════════════════════════════

        /// <summary>Trunca o rellena un texto al ancho indicado.</summary>
        static string TruncPad(string text, int width)
        {
            if (text.Length > width)
                return text.Substring(0, width - 1) + "…";
            return text.PadRight(width);
        }

        /// <summary>Calcula anchos óptimos de columna inspeccionando encabezados y datos.</summary>
        static int[] CalculateColumnWidths(List<string> columns, int rowLimit, int maxWidth)
        {
            var widths = new int[columns.Count];

            for (int c = 0; c < columns.Count; c++)
            {
                widths[c] = columns[c].Length; // mínimo = ancho del encabezado

                int rows = Math.Min(rowLimit, _gestor.TablaActual.Rows.Count);
                for (int r = 0; r < rows; r++)
                {
                    int len = (_gestor.TablaActual.Rows[r][columns[c]]?.ToString() ?? "").Length;
                    if (len > widths[c]) widths[c] = len;
                }

                // Limitar al máximo permitido
                if (widths[c] > maxWidth) widths[c] = maxWidth;
                if (widths[c] < 4) widths[c] = 4;
            }

            return widths;
        }

        /// <summary>Escribe texto en un color determinado y restablece el color.</summary>
        static void WriteColored(string text, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = prev;
        }
    }
}
