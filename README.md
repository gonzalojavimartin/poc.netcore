# KINESIS — PoC de API REST de pacientes

Prueba de concepto acotada para evaluar ASP.NET Core, Entity Framework Core y SQL Server como base del backend de KINESIS. Implementa alta, listado, consulta, modificación y baja lógica de pacientes.

## Requisitos y versiones

| Componente | Versión utilizada |
| --- | --- |
| SDK .NET | 10.0.401 |
| ASP.NET Core / runtime .NET | 10.0.12; proyecto `net10.0` |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.12 |
| Microsoft.EntityFrameworkCore.Design | 10.0.12 |
| dotnet-ef (herramienta local) | 10.0.12 |
| Swashbuckle.AspNetCore | 10.2.1 |
| SQL Server | 17.0.1135.8, Enterprise Developer Edition (64-bit) |

Se requiere .NET 10 SDK, SQL Server local en ejecución y un login SQL con los permisos necesarios. No es necesario instalar workloads adicionales de .NET. Las dependencias se obtienen desde NuGet.

## Estructura

```text
Kinesis.Poc.slnx
dotnet-tools.json
README.md
database/
  scripts/InitialCreate.idempotent.sql
src/Kinesis.Poc.Api/
  Controllers/PatientsController.cs
  Data/
    PatientsDbContext.cs
    PatientConfiguration.cs
    Migrations/
  Dtos/
    PatientRequest.cs
    PatientResponse.cs
  Errors/ApiExceptionHandler.cs
  Models/
    Patient.cs
    PatientStatus.cs
  Validation/PatientRequestValidator.cs
  Properties/launchSettings.json
  Program.cs
  appsettings.json
  appsettings.Development.json
  appsettings.example.json
```

La entidad representa los datos persistidos. La configuración define su correspondencia con SQL Server. El contexto permite consultar y guardar datos. Las migraciones mantienen el historial del esquema y el script SQL se deriva de ellas. Los controladores reciben y devuelven DTO, sin exponer directamente entidades de EF.

## SQL Server y conexión

La configuración utilizada es:

- Base: `KinesisPoCMicrosoft`.
- Autenticación SQL Server con usuario y contraseña.


Crear en Windows una **variable de entorno de usuario**:

```text
Nombre: KINESIS__ConnectionString_PatientsDb
Valor: Server=localhost;Database=KinesisPoCMicrosoft;User Id=<USUARIO_SQL>;Password=<CONTRASEÑA>;Encrypt=True;TrustServerCertificate=True;Persist Security Info=False
```

El login que ejecute migraciones debe poder crear la base si no existe y modificar su esquema. Para ejecutar el CRUD sobre una base preparada, necesita permisos de lectura, inserción y actualización sobre `Patients`; no se utiliza eliminación física.

## Restaurar y compilar

Ejecutar desde la raíz del repositorio, en PowerShell:

```powershell
dotnet tool restore
dotnet restore Kinesis.Poc.slnx
dotnet build Kinesis.Poc.slnx --no-restore
```

`dotnet tool restore` instala la versión de `dotnet-ef` declarada en `dotnet-tools.json`. Si falta .NET SDK o SQL Server, instalarlo por separado antes de continuar; estos comandos no los instalan.

## Base y migraciones

Las migraciones de EF Core son la única fuente de cambios estructurales del esquema. La API no ejecuta `EnsureCreated`, `Migrate` ni creación automática de base al iniciar.

### Crear la base y aplicar migraciones

Con la variable configurada, ejecutar:

```powershell
dotnet ef database update --project src/Kinesis.Poc.Api --startup-project src/Kinesis.Poc.Api
```

Si `KinesisPoCMicrosoft` no existe y el login tiene permisos, EF crea la base y aplica las migraciones pendientes. Si ya está actualizada, no repite las migraciones registradas en `__EFMigrationsHistory`. Este comando no ejecuta el archivo de `database/scripts`: genera SQL desde las clases de migración.

### Crear una nueva migración

Después de modificar el modelo de persistencia:

```powershell
dotnet ef migrations add NombreDelCambio --project src/Kinesis.Poc.Api --startup-project src/Kinesis.Poc.Api --output-dir Data/Migrations
```

Revisar y versionar la migración, sus metadatos y el snapshot antes de aplicarla. No modificar manualmente el esquema ni mantener SQL independiente de las migraciones.

### Generar SQL idempotente

```powershell
New-Item -ItemType Directory -Path database/scripts -Force
dotnet ef migrations script --idempotent --project src/Kinesis.Poc.Api --startup-project src/Kinesis.Poc.Api --output database/scripts/InitialCreate.idempotent.sql
```

El archivo incluido fue generado desde las migraciones y revisado. Comprueba el historial antes de aplicar los cambios. Espera una base existente y no contiene `CREATE DATABASE` ni credenciales. Su ejecución es una alternativa a aplicar las migraciones con EF; generar el archivo no ejecuta SQL sobre la base.

## Ejecutar la API y Swagger

Con la base preparada:

```powershell
dotnet run --project src/Kinesis.Poc.Api --launch-profile http --no-restore
```

Este comando compila antes de iniciar. Si ya se compiló el estado actual, puede usarse:

```powershell
dotnet run --project src/Kinesis.Poc.Api --launch-profile http --no-build --no-restore
```

Con `--no-build`, los cambios nuevos no se incorporan hasta compilar nuevamente. Detener la API con `Ctrl+C` antes de recompilar y reiniciar si es necesario.

El perfil HTTP de `Properties/launchSettings.json` define `http://localhost:5181` y el entorno Development.

- API: `http://localhost:5181/api/patients`.
- Swagger UI: `http://localhost:5181/api/docs`.
- Documento OpenAPI: `http://localhost:5181/swagger/v1/swagger.json`.
