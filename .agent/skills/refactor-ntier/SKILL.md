---
name: refactor-to-ntier-architecture
description: Activa este skill para refactorizar la solución y eliminar la duplicación de código, migrando hacia una Arquitectura Multicapa (N-Tier) mediante un proyecto "Shared" o "Core".
---

# Skill de Arquitectura: Migración N-Tier (Estándar Data Fusion Arena)

## Contexto
Actualmente, la solución sufre de una violación crítica del principio DRY. Las carpetas de lógica de negocio (`Models`, `Data`, `Processing`, `Services`) están duplicadas exactamente igual en los proyectos `DataArenaFusion` (WinForms) y `DataArenaFusion_Web`. 

El objetivo es extraer toda esta lógica a un proyecto centralizado de Biblioteca de Clases (Class Library) para lograr separación de responsabilidades y reutilización de código en múltiples interfaces (Escritorio, Web y Consola).

## Protocolo de Refactorización (Pasos de Ejecución)

Cuando el usuario pida aplicar la arquitectura N-Tier, debes ejecutar estrictamente las siguientes fases en orden:

### Fase 1: Creación del Núcleo Compartido
1. Crea un nuevo proyecto de tipo Class Library en la raíz de la solución llamado `DataArenaFusion.Core`.
   - Comando CLI esperado: `dotnet new classlib -n DataArenaFusion.Core`
2. Agrega este nuevo proyecto a la solución principal.
   - Comando CLI esperado: `dotnet sln add DataArenaFusion.Core/DataArenaFusion.Core.csproj`

### Fase 2: Migración de Lógica (El corazón del sistema)
Mueve las siguientes carpetas desde `DataArenaFusion_Web` (o WinForms) directamente hacia la raíz del nuevo proyecto `DataArenaFusion.Core`:
- `/Models` (Modelos de datos y entidades)
- `/Data` (Lectores, Interfaces e ImportadorTabular)
- `/Processing` (Interfaces, Algoritmos y clases de agrupamiento/ordenamiento)
- `/Services` (GestorDatos, Database Connection Services, y Factories)

*Regla crítica:* Conserva intactos todos los patrones de diseño existentes (POO, Interfaces, Factory pattern). Solo estás moviendo los archivos de lugar.

### Fase 3: Limpieza y Referencias
1. **Elimina** las carpetas `/Models`, `/Data`, `/Processing` y `/Services` de los proyectos `DataArenaFusion` (WinForms) y `DataArenaFusion_Web`.
2. Añade una referencia del proyecto `Core` a las aplicaciones UI.
   - CLI: `dotnet add DataArenaFusion/DataArenaFusion.csproj reference DataArenaFusion.Core/DataArenaFusion.Core.csproj`
   - CLI: `dotnet add DataArenaFusion_Web/DataArenaFusion_Web.csproj reference DataArenaFusion.Core/DataArenaFusion.Core.csproj`

### Fase 4: Reparación de Namespaces
- Actualiza los `using` en los archivos de interfaz gráfica (`Form1.cs`, `Controllers`, etc.) para que apunten al nuevo namespace `DataArenaFusion.Core.*`.
- Asegúrate de que la Inyección de Dependencias en el `Program.cs` de la WebApp registre los servicios apuntando a las clases del proyecto `Core`.

### Fase 5: Expansión de Ecosistema (Consola)
- Si el usuario lo requiere, crea un tercer proyecto `DataArenaFusion.Console` que referencie a `Core` y utilice la lógica para operar mediante CLI, logrando paridad total en 3 plataformas.

## Reglas de Arquitectura Strictas
1. **Zero UI Logic in Core:** El proyecto `DataArenaFusion.Core` NUNCA debe contener referencias a `System.Windows.Forms` ni a `Microsoft.AspNetCore.Mvc`.
2. **Zero Business Logic in UI:** Los botones de WinForms o los Endpoints de la Web NUNCA deben procesar datos crudos, siempre deben instanciar las interfaces (ej. `IProcesadorDatos`) provistas por el Core.