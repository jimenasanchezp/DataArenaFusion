# Data Arena Fusion 🚀

**Ingeniera responsable:** Jimena Monzerrat Sanchez Palos 
**Colaboradores:** Carolina Sustaita de Luna
**Institución:** Instituto Tecnológico Superior de Monclova (TecNM)

Data Arena Fusion es una plataforma integral diseñada para la ingesta, procesamiento y migración de datos heterogéneos. El sistema permite consolidar información de diversas fuentes (CSV, JSON, XML, TXT) y transformarla mediante algoritmos optimizados de ordenamiento y análisis, facilitando su migración final a bases de datos relacionales como MariaDB y PostgreSQL.

---

## 📂 Estructura del Proyecto

El ecosistema está dividido en una arquitectura robusta que separa la lógica de negocio de la presentación, asegurando la escalabilidad entre las versiones de escritorio y web.

```text
DataArenaFusion/
├── DataArenaFusion.slnx                     ← Solución principal
│
├── DataArenaFusion/                         ← Aplicación Desktop (WinForms)
│   ├── Models/
│   │   ├── Registro.cs                      ← Modelo universal con soporte para columnas dinámicas
│   │   └── TablaImportada.cs                ← Estructura para la conversión a DataTable
│   ├── Data/
│   │   ├── ImportadorTabular.cs             ← Motor de parseo para formatos planos
│   │   └── Interfaces/                      ← Nivel 2: Fábrica de lectores especializados
│   ├── Processing/                          ← Nivel 4-5: Núcleo de Algoritmos
│   │   ├── Algoritmos/
│   │   │   ├── OrdenadorDatos.cs            ← Implementación de BubbleSort/QuickSort (Sin LINQ)
│   │   │   ├── Agrupador.cs                 ← Lógica de agregación por categorías
│   │   │   └── DetectorDuplicados.cs        ← Detección de colisiones de IDs
│   │   └── Procesadores/                    ← Orquestadores de procesamiento asíncrono
│   ├── Services/
│   │   ├── GestorDatos.cs                   ← Controlador de estado y memoria principal
│   │   └── Database/                        ← Nivel 3: Conectores y Migradores SQL
│   └── Form1.cs                             ← Interfaz con Consola ASCII y Gráficas
│
└── DataArenaFusion_Web/                     ← Aplicación Web (ASP.NET Core MVC)
    ├── Controllers/
    │   └── DataApiController.cs             ← API de carga y procesamiento remoto
    ├── Services/
    │   └── DataEnricherService.cs           ← Enriquecimiento vía APIs externas (Divisas/GPS)
    ├── Views/                               ← Vistas dinámicas en Razor
    └── wwwroot/                             ← Recursos estáticos (CSS/JS)
