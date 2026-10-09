# Design Authority

Aplicación web personal para el seguimiento de arquitectura, implementada con
ASP.NET Core MVC en .NET 10, SQL Server, Dapper y Bootstrap. La solución sigue
la separación Web, Application, Domain e Infrastructure.

## Configuración local

Requiere .NET 10 SDK y SQL Server. Configure estos valores fuera del repositorio:

- `ConnectionStrings__DesignAuthority`: cadena de conexión a SQL Server.
- `ConfiguredUser__Id`: Id de usuario presente en `dbo.ConfiguredUser`.
- `FileStorage__RootPath`: raíz local para los archivos de Minuta.

El script versionado `database/scripts/001-initial-schema.sql` crea el esquema
y los catálogos iniciales. No crea un Trámite predeterminado; debe existir un
Trámite de prueba/operación configurado manualmente si el flujo lo requiere.

## Compilar y probar

Desde la raíz del repositorio:

```powershell
dotnet restore .\DesignAuthority.sln
dotnet build .\DesignAuthority.sln --no-restore
dotnet test .\DesignAuthority.sln --no-build
```

Las pruebas de dominio, aplicación y almacenamiento local se ejecutan sin
SQL Server. Las pruebas de repositorios y escenarios end-to-end que requieren
SQL Server deben ejecutarse contra una base desechable con el script inicial
aplicado; no se debe interpretar una compilación exitosa como validación del
DDL o de las transacciones en SQL Server.

La especificación, decisiones y escenarios de aceptación están en
[`specs/003-design-authority-mvp/`](./specs/003-design-authority-mvp/).
