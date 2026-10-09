# Research: Equipos y Tecnologías

## Identificador de Persona

- **Decision**: Usar `int IDENTITY(1,1)` de SQL Server como clave primaria
  técnica de Persona. Tratarla como inmutable en los casos de uso y no mostrar
  una opción de edición.
- **Rationale**: La especificación requiere Id único e inmutable, pero difiere
  el mecanismo técnico al plan. SQL Server IDENTITY ofrece generación simple
  en inserción y una PK crea unicidad. No se requiere continuidad numérica ni
  que el Id tenga semántica de negocio.
- **Alternatives considered**: GUID generado por la aplicación o por SQL,
  secuencia explícita y clave natural introducen configuración, superficie o
  reglas de unicidad no requeridas. Una identidad de 64 bits no está justificada
  por volumen conocido; la capacidad final se puede reconsiderar si el modelo
  integral define otro rango.
- **Source**: [IDENTITY (Property) - SQL Server](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql-identity-property?view=sql-server-ver17);
  [Primary and foreign key constraints - SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver17).

## Unicidad de asignación para roles de liderazgo

- **Decision**: Validar la capacidad de asignación en Application/Domain y
  serializar escritura en una transacción SQL Server bloqueando la fila de
  referencia del Role. Consultar ocupación activa y escribir antes de terminar
  la transacción.
- **Rationale**: El requisito difiere: líderes admiten cero o una persona
  activa y otros roles admiten múltiples. Un índice único filtrado directamente
  sobre `Person.RoleId` trataría por igual ambos tipos. SQL Server índices
  filtrados seleccionan filas mediante predicado sobre la tabla indexada; no
  permiten derivar el filtro consultando `Role`. El bloqueo por Role conserva
  la regla en altas, reasignaciones y bajas concurrentes sin desnormalizar
  `IsLeadership` en Persona.
- **Alternatives considered**: Un índice único para todo Role impediría más de
  un Desarrollador/Arquitecto/Release manager/Tester; duplicar bandera de
  liderazgo en Persona crea estado redundante; no serializar la validación
  permitiría dos altas concurrentes.
- **Source**: [Create filtered indexes - SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/create-filtered-indexes?view=sql-server-ver17).

## Integridad entre Equipo o Tecnología y Rol

- **Decision**: Mantener `TeamTechnology` y `Role` como catálogos separados;
  cada Role referencia su opción y `TeamTechnology` mantiene un campo
  obligatorio `Kind` con valores `Technology` o `Team`. Persona utiliza una FK
  compuesta `(TeamTechnologyId, RoleId)` y puede pertenecer a cualquiera de
  las ocho opciones.
- **Rationale**: Un catálogo unificado simplifica la asignación única de
  Persona, mientras `Kind` preserva que Tecnología y Equipo no son conceptos
  semánticamente intercambiables. Para que las features posteriores puedan
  asociar Hallazgos, Acuerdos, Decisiones de Arquitectura y Reuniones solo a
  Tecnologías, el catálogo debe exponer clave candidata
  `(TeamTechnologyId, Kind)`. Las entidades consumidoras podrán referenciar
  dicha pareja con `Kind` fijado y restringido a `Technology`, haciendo
  verificable el límite también en relaciones de persistencia.
- **Alternatives considered**: Dos catálogos independientes duplicarían el
  patrón de asignación de Persona sin necesidad funcional. Omitir `Kind`
  perdería la distinción y dificultaría restringir las features consumidoras.
  Validar solo por etiqueta o en la pantalla no garantiza integridad de las
  asociaciones futuras.
- **Source**: [Primary and foreign key constraints - SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver17).

## Fecha de registro y auditoría

- **Decision**: Persistir un solo `CreatedAtUtc datetime2` para el evento de
  alta. Application asigna el instante UTC al crear y conserva el valor al
  modificar; la UI presenta la fecha funcional según formato de aplicación.
- **Rationale**: La especificación permite que Fecha de registro coincida con
  FechaCreacion y prohíbe mantener dos valores diferentes para el mismo evento.
  Una sola columna evita divergencias. Si el esquema integral decide política
  de reloj común, esta columna debe alinearse con el modelo de auditoría.
- **Alternatives considered**: Columnas duplicadas `RegistrationDate` y
  `CreatedAt` para el mismo evento permiten inconsistencias y no aportan
  información funcional.
- **Source**: Reglas de auditoría y fecha de alta en
  [spec.md](./spec.md), FR-010 y FR-013.

## Arquitectura y validación MVC

- **Decision**: Mantener páginas y ViewModels en Web, validación de interacción
  en servidor, casos de uso y contratos en Application, invariantes puras en
  Domain y SQL parametrizado Dapper en Infrastructure.
- **Rationale**: Conserva la arquitectura de cuatro capas y aplica el
  Repository Pattern y DI definidos por la Constitution. MVC valida model
  binding/model state; las reglas que dependen de Role y ocupación deben
  repetirse en Application/Domain antes de persistir.
- **Alternatives considered**: SQL desde Controllers o reglas únicamente
  cliente no son aceptables por separación de capas y validación server-side.
- **Sources**:
  - [Model validation in ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)
  - [Dapper](https://github.com/DapperLib/Dapper)
  - [Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-10.0)

## Datos iniciales

- **Decision**: Versionar en scripts SQL los ocho registros TeamTechnology y
  quince Role definidos en FR-005/FR-007. Incluir clave estable/código,
  etiqueta de presentación, `Kind` (`Technology` o `Team`) y clasificación de
  ocupación según corresponda. Appian, Databricks, Microservicios y Power BI
  son `Technology`; Arquitectura, Proyecto, Célula y QA son `Team`. No ofrecer
  mantenimiento dinámico de catálogos en esta feature.
- **Rationale**: La specification fija el conjunto inicial y las asociaciones
  válidas, y las features posteriores dependen de diferenciar qué opciones son
  Tecnologías.
- **Alternatives considered**: Crear roles automáticamente a partir de texto
  introducido por la persona permitiría asignaciones no definidas y divergencia
  de etiquetas.

## Contexto del repositorio y límites de diseño

- **Decision**: Documentar una solución MVC de cuatro proyectos y planificar
  pruebas unitarias y de integración, sin atribuir un framework existente al
  repositorio.
- **Rationale**: Al momento del plan no hay `.sln`, `.csproj` ni convenciones
  de pruebas detectadas. Framework, entorno de SQL Server y comandos exactos
  se seleccionan cuando se cree la solución.
- **Alternatives considered**: Suponer que la aplicación ya compila o que hay
  herramientas de test instaladas no está respaldado por el repositorio.
