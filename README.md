# Data Arena Fusion 🚀

Sistema integral de integración, procesamiento y visualización de datos heterogéneos provenientes de múltiples fuentes. Este proyecto ha sido desarrollado siguiendo una **Arquitectura N-Tier (Multicapa)** para garantizar la escalabilidad y reutilización de código.

## 🎯 Cumplimiento de Requisitos

A continuación se detallan los niveles del reto completados:

### 🟢 Nivel 1 – Recolección de datos
- [x] **Datasets reales:** Integración de datos de Amazon (CSV), Clima (JSON) y Estudiantes (XML).
- [x] **Formatos soportados:** JSON, CSV, XML y TXT.
- [x] **Estructuras:** Manejo de datos tabulares, jerárquicos y texto plano.

### 🟡 Nivel 2 – Lectura de archivos
- [x] **Lectores especializados:** Implementación de `CsvReader`, `JsonReader`, `XmlReader` y `TxtReader`.
- [x] **Modelo Común:** Todos los datos se transforman a la clase unificada `Registro`.

### 🔵 Nivel 3 – Bases de datos
- [x] **Conectividad:** Soporte para **MariaDB** y **PostgreSQL**.
- [x] **Migración:** Capacidad de leer archivos y migrar la información directamente a tablas de base de datos.

### 🟣 Nivel 4 – Administración y Organización
- [x] **Almacenamiento:** Uso de `List<T>` como repositorio principal en memoria.
- [x] **Acceso Rápido:** Implementación de `Dictionary<int, Registro>` para búsquedas instantáneas $O(1)$ por ID.

### 🔴 Nivel 5 – Procesamiento de Datos
- [x] **Filtrado:** Búsqueda dinámica sobre todas las columnas.
- [x] **Ordenamiento:** Implementación de algoritmos de ordenamiento (BubbleSort / QuickSort) sin depender de LINQ.
- [x] **Agrupamiento:** Organización de datos por categorías usando diccionarios.
- [x] **Duplicados:** Detección y conteo de registros repetidos basados en ID o columnas específicas.

### 🟠 Nivel 6 – Visualización
- [x] **Consola:** Interfaz de línea de comandos con tablas de datos y gráficas de barras en formato ASCII.
- [x] **WinForms:** Aplicación de escritorio con `DataGridView` y control `Chart` para gráficas de barras y pastel.

### ⚡ Bonus (Extras)
- [x] **LINQ:** Aplicado para consultas complejas y transformaciones de datos internas.
- [x] **ASP.NET Core MVC:** Aplicación web moderna con carga de archivos y visualización dinámica.
- [x] **API REST:** Consumo de servicios externos para enriquecimiento de datos (Tipos de cambio USD/MXN y Geocodificación).

---

## 🏗️ Estructura de la Solución

```text
DataArenaFusion/
├── DataArenaFusion.slnx          ← Solución principal
│
├── DataArenaFusion.Core/         ← Núcleo de Lógica (Arquitectura N-Tier)
│   ├── Models/
│   │   ├── Registro.cs           ← Modelo universal con soporte dinámico
│   │   └── TablaImportada.cs     ← Estructura para conversión a DataTable
│   ├── Data/
│   │   ├── ImportadorTabular.cs  ← Motor de parseo para formatos planos
│   │   └── Interfaces/           ← Nivel 2: Fábrica de lectores especializados
│   ├── Processing/
│   │   ├── Algoritmos/           ← Nivel 4-5: Núcleo de Algoritmos (Sin LINQ)
│   │   │   ├── OrdenadorDatos.cs ← Implementación de BubbleSort/QuickSort
│   │   │   ├── Agrupador.cs      ← Lógica de agregación por categorías
│   │   │   └── DetectorDuplicados.cs ← Detección de colisiones de IDs
│   │   └── Procesadores/         ← Orquestadores de procesamiento asíncrono
│   └── Services/
│       ├── GestorDatos.cs        ← Controlador de estado y memoria principal
│       └── Database/             ← Nivel 3: Conectores y Migradores SQL
│
├── DataArenaFusion/              ← Aplicación Desktop (WinForms)
│   ├── Form1.cs                  ← Interfaz principal con gráficas y tablas
│   └── TestData/                 ← Datasets reales (Nivel 1)
│
├── DataArenaFusion.Console/      ← Aplicación de Consola
│   └── Program.cs                ← Interfaz ASCII y pruebas rápidas
│
└── DataArenaFusion_Web/          ← Aplicación Web (ASP.NET Core MVC)
    ├── Controllers/
    │   └── DataApiController.cs  ← API de carga y procesamiento remoto
    ├── Services/
    │   └── DataEnricherService.cs ← Enriquecimiento vía APIs (Bonus)
    └── Views/                    ← Vistas dinámicas en Razor
```

---

## 🛠️ Tecnologías Utilizadas

- **Lenguaje:** C# (.NET 8.0)
- **UI Desktop:** Windows Forms
- **Web:** ASP.NET Core MVC / Bootstrap / JavaScript
- **Gráficas:** System.Windows.Forms.DataVisualization / Chart.js (Web)
- **Base de Datos:** MySql.Data (MariaDB) y Npgsql (PostgreSQL)
- **APIs Externas:** Open Exchange Rates y Nominatim (OpenStreetMap)

---

## 🚀 Instalación y Uso

1. Clonar el repositorio.
2. Abrir la solución `DataArenaFusion.slnx` en Visual Studio 2022.
3. Compilar la solución para restaurar paquetes NuGet.
4. Ejecutar cualquiera de los tres proyectos de inicio (`WinForms`, `Console` o `Web`).
