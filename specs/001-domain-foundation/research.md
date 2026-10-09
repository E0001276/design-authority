# Research: Fundamentos de Dominio

## Arquitectura MVC y separación de responsabilidades

- **Decision**: Usar ASP.NET Core MVC para las páginas y los flujos internos,
  con Controllers, Views y ViewModels confinados a `DesignAuthority.Web`.
- **Rationale**: MVC separa responsabilidades de presentación y permite que la
  lógica de dominio se pruebe sin depender de la UI. La guía oficial de MVC
  describe las responsabilidades de Controller, Model y View y la separación
  de intereses.
- **Alternatives considered**: API REST con cliente separado (excluida por
  Constitution y spec); lógica de negocio dentro de Controllers (rechazada por
  las capas obligatorias).
- **Source**: [Overview of ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-10.0)

## Dependency Injection

- **Decision**: Registrar servicios de aplicación, repositorios y servicios de
  almacenamiento con el contenedor de DI de ASP.NET Core; las dependencias
  entran por constructores.
- **Rationale**: ASP.NET Core soporta DI y composición de servicios; esto
  implementa el requisito constitucional y evita instanciación acoplada.
- **Alternatives considered**: Service Locator o construcción manual distribuida
  (rechazadas por ocultar dependencias y debilitar la consistencia de DI).
- **Source**: [Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-10.0)

## SQL Server y Dapper

- **Decision**: Implementar acceso a datos en Infrastructure con Dapper sobre
  conexiones ADO.NET, repositorios explícitos y consultas parametrizadas.
- **Rationale**: Dapper extiende las conexiones `DbConnection` para ejecutar
  SQL y mapear filas a objetos; es coherente con SQL Server y excluye Entity
  Framework. Restricciones PK/FK de SQL Server permiten proteger integridad
  relacional confirmada en la base.
- **Alternatives considered**: Entity Framework (prohibido); SQL directo en
  Application/Web (viola Repository Pattern y límites de capa).
- **Sources**:
  - [Dapper](https://github.com/DapperLib/Dapper)
  - [Primary and foreign key constraints - SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver17)

## Diseño de datos ante reglas diferidas

- **Decision**: Modelar todos los conceptos solicitados y solo materializar las
  relaciones y atributos confirmados en la especificación. Para otros campos,
  documentar el dato como pendiente y no elegir relaciones ni cardinalidades.
- **Rationale**: La especificación prohíbe inferir reglas. La constitución exige
  un esquema inicial integral antes de implementar features dependientes; por
  tanto, el esquema de producción debe quedar bloqueado hasta que las features
  dueñas definan atributos y relaciones faltantes.
- **Alternatives considered**: Añadir campos genéricos o relaciones N:M por
  conveniencia (rechazadas por inventar semántica); excluir entidades futuras
  del inventario (rechazada porque la solicitud pide contemplar todos los
  conceptos).

## Minuta y almacenamiento

- **Decision**: Representar una Minuta vigente por Reunión, mantener solo el
  archivo más reciente y almacenar una referencia relativa junto con el nombre
  original y el identificador/nombre físico. Encapsular almacenamiento en una
  interfaz de Infrastructure.
- **Rationale**: Cumple la especificación y permite que la ubicación física no
  se filtre al dominio. No implementar versionado ni retención histórica.
- **Alternatives considered**: Rutas absolutas o historial de archivos
  (ambas contradicen la especificación).
- **Open decision**: Política de compensación/recuperación si el reemplazo del
  archivo y la actualización de SQL fallan parcialmente; la especificación no
  define este comportamiento y se debe resolver antes de implementar ese flujo.

## Pruebas y herramientas del repositorio

- **Decision**: Separar pruebas unitarias de Domain/Application y pruebas de
  integración de Infrastructure contra SQL Server; elegir el framework y el
  mecanismo de provisioning al crear la solución.
- **Rationale**: El repositorio de partida no contiene `.sln`, `.csproj` ni
  convenciones de test existentes, así que no se afirma que una herramienta
  esté instalada o se use hoy.
- **Alternatives considered**: Afirmar una suite o comando de test sin evidencia
  del repositorio (rechazado).
- **Open decisions**: framework de pruebas, versión concreta de SQL Server para
  CI/pruebas y método de ejecutar scripts.
