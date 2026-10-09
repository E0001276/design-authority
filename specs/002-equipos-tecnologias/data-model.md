# Data Model: Equipos y Tecnologías

## Alcance y estado

Modelo técnico propuesto para los datos definidos por
[spec.md](./spec.md). No es un script SQL. Los scripts de creación deben
integrarse al esquema inicial integral de Design Authority y aprobarse antes
de implementar persistencia, según la Constitution. El modelo no añade
asignaciones múltiples ni gestión dinámica de catálogos.

Los tipos y longitudes no especificados a continuación son propuestas
técnicas para revisión al consolidar el esquema completo.

## Entidades

### TeamTechnology (catálogo unificado con tipo explícito)

Catálogo unificado de las ocho opciones iniciales, con:

| Campo lógico | Propuesta técnica | Regla |
|---|---|---|
| TeamTechnologyId | `int`, PK | Clave técnica. |
| Code | `nvarchar`, único | Código estable para scripts, seed y referencias; no es editable por la pantalla de Personas. |
| Name | `nvarchar`, único | Etiqueta: Appian, Databricks, Microservicios, Power BI, Arquitectura, Proyecto, Célula o QA. |
| Kind | `varchar(10)` o equivalente, NOT NULL | Clasificación explícita: `Technology` o `Team`; limitar a esos valores. |

El catálogo unificado mantiene la semántica diferenciada mediante `Kind`, sin
convertir Equipos y Tecnologías en sinónimos ni requerir dos estructuras de
asignación para Persona. La clave primaria es `TeamTechnologyId`; exponer
además `(TeamTechnologyId, Kind)` como clave candidata permite a las features
posteriores imponer asociaciones Technology-only mediante relaciones
compuestas.

Clasificación inicial:

| TeamTechnology | Kind |
|---|---|
| Appian | Technology |
| Databricks | Technology |
| Microservicios | Technology |
| Power BI | Technology |
| Arquitectura | Team |
| Proyecto | Team |
| Célula | Team |
| QA | Team |

### Role (Rol o puesto)

Catálogo de Roles asociados a exactamente un TeamTechnology:

| Campo lógico | Propuesta técnica | Regla |
|---|---|---|
| RoleId | `int`, PK | Clave técnica. |
| TeamTechnologyId | `int`, FK | Equipo o Tecnología al que pertenece el Role. |
| Code | `nvarchar` | Identificador estable dentro del catálogo. |
| Name | `nvarchar` | Etiqueta mostrada al usuario. |
| IsLeadership | `bit` | Si es verdadero, admite cero o una Persona activa; si no, cero o más. |

Claves candidatas: `(TeamTechnologyId, RoleId)` para la FK de Persona y
unicidad de `(TeamTechnologyId, Code)` y `(TeamTechnologyId, Name)`.

Roles iniciales (15):

| Equipo o Tecnología | Role | IsLeadership |
|---|---|---:|
| Appian | Líder técnico | Sí |
| Appian | Desarrollador | No |
| Databricks | Líder técnico | Sí |
| Databricks | Desarrollador | No |
| Microservicios | Líder técnico | Sí |
| Microservicios | Desarrollador | No |
| Power BI | Líder técnico | Sí |
| Power BI | Desarrollador | No |
| Arquitectura | Líder de arquitectura | Sí |
| Arquitectura | Arquitecto | No |
| Proyecto | Líder de proyecto | Sí |
| Proyecto | Release manager | No |
| Célula | Líder de célula | Sí |
| QA | Líder de QA | Sí |
| QA | Tester | No |

Los catálogos y sus relaciones se versionan como datos de referencia. No se
define en esta feature una pantalla CRUD para editarlos ni reglas futuras para
añadir opciones.

### Person (Persona)

| Campo lógico | Propuesta técnica | Regla |
|---|---|---|
| PersonId | `int IDENTITY(1,1)`, PK | Id único e inmutable. Application no lo acepta en modificación. |
| Name | `nvarchar` | Obligatorio; validar no vacío en servidor. |
| PaternalSurname | `nvarchar` | Obligatorio; validar no vacío en servidor. |
| MaternalSurname | `nvarchar`, nullable | Opcional. |
| AbbreviatedName | `nvarchar` | Obligatorio; validar no vacío en servidor. |
| TeamTechnologyId | `int`, NOT NULL, FK compuesta | Exactamente una opción. |
| RoleId | `int`, NOT NULL, FK compuesta | Exactamente un Role válido para el TeamTechnology. |
| CreatedAtUtc | `datetime2`, NOT NULL | Instante UTC de alta; es también Fecha de registro, inmutable. |
| CreatedByUserId | Tipo según modelo común | Usuario configurado que crea el registro. |
| UpdatedAtUtc | `datetime2`, nullable | Última modificación, si la hubo. |
| UpdatedByUserId | Tipo según modelo común, nullable | Usuario configurado que modificó, si aplica. |
| IsActive | `bit`, NOT NULL | `true` al alta; baja lógica lo establece en `false`. |
| DeletedAtUtc | `datetime2`, nullable | Instante UTC de baja lógica. |
| DeletedByUserId | Tipo según modelo común, nullable | Usuario configurado que ejecutó la baja. |

La estrategia `IDENTITY` es decisión técnica de este plan (ver
[research.md](./research.md)); no se añade a la especificación funcional. Se
recomienda que las operaciones ordinarias no permitan cambiar PersonId.

## Relaciones y cardinalidad

| Origen | Relación | Destino | Cardinalidad/regla |
|---|---|---|---|
| Person | pertenece a | TeamTechnology | Cada persona tiene exactamente uno; cada opción puede tener cero o más personas. |
| Person | tiene | Role | Cada persona tiene exactamente uno; Role debe estar asociado al mismo TeamTechnology. |
| Role | válido para | TeamTechnology | Cada Role pertenece a exactamente un equipo/tecnología; cada opción tiene los Roles iniciales de FR-007. |

No hay tablas de asignación múltiple Persona-Equipo o Persona-Rol. Persona
puede referenciar cualquiera de las ocho filas del catálogo unificado. La FK
compuesta propuesta `(Person.TeamTechnologyId, Person.RoleId)` referencia
`Role(TeamTechnologyId, RoleId)` para rechazar combinaciones incompatibles
también al nivel de persistencia.

### Límite de asociación para features futuras

Hallazgos, Acuerdos, Decisiones de Arquitectura y Reuniones deberán asociarse
únicamente a `TeamTechnology` con `Kind = Technology`, conforme a
`001-domain-foundation`. El modelo conserva explícitamente ese dato y
proporciona la clave candidata `(TeamTechnologyId, Kind)` para que cada futura
entidad consumidora pueda:

1. Almacenar la referencia al `TeamTechnologyId` y la clasificación
   correspondiente.
2. Restringir esa clasificación a `Technology`.
3. Referenciar `(TeamTechnologyId, Kind)` mediante FK compuesta.

Así, la FK impide que se asocien valores clasificados como `Team`; la
restricción no depende solo del formulario o de comparar etiquetas. El diseño
detallado de las entidades consumidoras permanece en sus features.

## Reglas de integridad y validación

1. **Campos obligatorios**: Web valida el modelo de entrada y Application
   aplica las reglas antes del repositorio; `Name`, `PaternalSurname`,
   `AbbreviatedName`, `TeamTechnologyId` y `RoleId` requeridos; apellido
   materno nullable.
2. **Catálogo válido**: rechazar opciones o Roles desconocidos. `Kind` solo
   admite `Technology` o `Team`; Persona acepta ambas clasificaciones. La
   tabla de referencia y la FK compuesta aseguran que el Role seleccionado
   pertenece al TeamTechnology.
3. **Liderazgo**: el Domain/Application clasifica el Role y presenta error de
   negocio si ya existe titular activo. El repositorio ejecuta alta o cambio
   de asignación en una transacción SQL Server, bloquea la fila estable de
   `RoleId`, comprueba la ocupación y escribe antes de liberar el bloqueo.
   Altas, reasignaciones y bajas deben seguir el mismo protocolo para evitar
   asignaciones duplicadas concurrentes. Para cambios de Role se bloquean los
   RoleIds involucrados en orden ascendente antes de validar/escribir.
4. **Roles múltiples**: no se impone exclusividad para Desarrollador,
   Arquitecto, Release manager o Tester.
5. **Id inmutable**: generar `PersonId` al insertar; comandos de modificación
   no reciben un Id modificable y filtran por la clave del registro objetivo.
6. **Fecha de registro**: asignar `CreatedAtUtc` al alta; usar el mismo valor
   como fecha funcional de registro, preservarlo en update y no mantener otra
   columna duplicada para el mismo evento.
7. **Soft delete**: la baja actualiza `IsActive`, `DeletedAtUtc`,
   `DeletedByUserId`, `UpdatedAtUtc` y `UpdatedByUserId`; el listado funcional
   filtra activos. El rol de liderazgo queda vacante una vez desactivada la
   Persona. No se define restauración.
8. **Auditoría**: cada alta/modificación/baja usa el usuario configurado del
   servicio común. El tipo/FK exacto del usuario se alinea con el modelo común
   de `001-domain-foundation` al consolidar el esquema.

Un índice único filtrado directamente por `RoleId` y `IsActive` no es
adecuado: limitaría también los Roles de asignación múltiple. No duplicar
`IsLeadership` en Person para fingir ese filtro. La transacción con bloqueo por
Role es la barrera de concurrencia propuesta.

## Transiciones

| Operación | Estado resultante |
|---|---|
| Alta | Persona activa con Id nuevo, fecha de registro/creación, usuario de creación y Role válido. |
| Modificación | Campos funcionales y asociación pueden cambiar; Id y fecha de registro permanecen; actualizar auditoría de modificación. |
| Baja lógica | Inactiva con fecha y usuario de eliminación; deja de ocupar capacidad de Role de liderazgo y no aparece en el directorio activo. |

## Decisiones de esquema que deben verificarse al integrarlo

- El formato/tipo de usuario auditor debe coincidir con el usuario configurado
  definido en el esquema integral.
- Revisar longitudes y restricciones de texto con criterios comunes de toda la
  aplicación; la especificación no fija máximos.
- Confirmar tipos exactos, nombres físicos, índices de lectura y estrategia de
  scripts en el plan/tareas de esquema integral.
- No ejecutar scripts específicos de feature antes de esa aprobación.
