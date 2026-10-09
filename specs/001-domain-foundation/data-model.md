# Data Model: Fundamentos de Dominio

## Alcance y estado

Este documento enumera los conceptos y relaciones confirmados para el esquema
inicial. No es un script SQL ni una aprobación para crear tablas todavía. Los
atributos de negocio que la especificación no define se marcan como
**pendientes** y deben detallarse en las features dueñas antes de aprobar el
esquema integral, conforme a la Constitution.

Cada entidad persistente necesitará una clave técnica y los campos de auditoría
que apliquen según la especificación. Tipo de clave, tipos SQL, nulabilidad y
reglas de unicidad no funcionales quedan para el diseño de esquema; no se
inventan aquí.

## Entidades y atributos confirmados

| Entidad | Atributos/reglas confirmados | Pendiente de definición |
|---|---|---|
| Trámite | Activo; indicador de predeterminado sujeto a máximo uno entre los activos; auditoría según aplique. | Nombre, otros atributos, reglas completas para desactivar/cambiar cuando existan registros asociados. |
| Subetapa | Pertenece a exactamente un Trámite; solo Subetapas activas del Trámite se ofrecen para Hallazgo. | Nombre, otros atributos y ciclo de vida adicional. |
| Tecnología | Catálogo inicial: Appian, Databricks, Microservicios, Power BI. | Atributos adicionales y reglas de mantenimiento del catálogo. |
| Usuario configurado | Identidad usada para asociar actividad y auditoría; no hay autenticación. | Forma de configuración, atributos y almacenamiento exacto. |
| Hallazgo | Pertenece a un Trámite y usa una Subetapa del mismo Trámite; pertenece a una Tecnología; cero o más seguimientos; auditoría y soft delete cuando apliquen. | Atributos propios, estatus y reglas de eliminación. |
| Seguimiento de Hallazgo | Registro independiente con fecha, usuario, observación; puede registrar estatus anterior/nuevo y fecha compromiso anterior/nueva cuando aplique. | Regla para diferenciar ausencia de cambio y valores nulos; demás atributos. |
| Acuerdo | Pertenece a un Trámite y una Tecnología; cero o más seguimientos; no se establece relación con Subetapa en esta feature. | Atributos propios, estatus y demás reglas. |
| Seguimiento de Acuerdo | Registro independiente asociado a un Acuerdo, con fecha, usuario y observación; puede registrar estatus anterior/nuevo y fecha compromiso anterior/nueva cuando aplique. | Regla para diferenciar ausencia de cambio y valores nulos; demás atributos. |
| Decisión de Arquitectura | Pertenece a un Trámite y una Tecnología; relación con Subetapa se difiere a su feature. | Atributos, asociación con Subetapa y ciclo de vida. |
| Reunión | Pertenece a un Trámite y una Tecnología; no se asocia a Subetapa; puede tener una Minuta; auditoría cuando aplique. | Atributos de reunión. |
| Minuta | Asociada a Reunión; como máximo una; nombre original, nombre físico o identificador interno, referencia relativa; solo el documento más reciente se conserva. | Límite/tipo de archivo, política de fallo al reemplazar y ubicación del storage por ambiente. |
| Informe Semanal | Concepto reconocido; la funcionalidad está fuera de alcance. | Atributos y relaciones para una feature posterior. No crear estructura funcional en esta feature. |
| Persona | Concepto del dominio; auditoría si se modifica. | Atributos y toda relación/cardinalidad con Equipo o Rol. |
| Equipo | Concepto del dominio; auditoría si se modifica. | Atributos y relaciones con Persona, Tecnología u otras entidades. |
| Rol o puesto | Concepto del dominio; auditoría si se modifica. | Atributos y todas las relaciones/asignaciones. |

No se fija relación alguna entre Persona, Equipo y Rol en este diseño.

## Relaciones confirmadas

| Origen | Relación | Destino | Regla |
|---|---|---|---|
| Subetapa | pertenece a | Trámite | Cada Subetapa pertenece a un solo Trámite; un Trámite puede tener cero o más Subetapas. |
| Hallazgo | pertenece a | Trámite | Obligatoria según la especificación. |
| Hallazgo | utiliza | Subetapa | Pertenece al mismo Trámite; solo una Subetapa activa de ese Trámite se ofrece. |
| Acuerdo, Decisión de Arquitectura, Reunión | pertenece a | Trámite | Registros funcionales dentro del contexto de Trámite, según Key Entities de la especificación. |
| Hallazgo, Acuerdo, Decisión de Arquitectura, Reunión | pertenece a | Tecnología | Cada registro se asocia a una Tecnología; una Tecnología puede tener varios registros. |
| Seguimiento de Hallazgo | asociado a | Hallazgo | Un Hallazgo puede tener cero o más seguimientos independientes. |
| Seguimiento de Acuerdo | asociado a | Acuerdo | Un Acuerdo puede tener cero o más seguimientos independientes. |
| Minuta | asociada a | Reunión | Una Reunión puede tener cero o una Minuta; reemplazar elimina la anterior y conserva la más reciente. |

Las relaciones entre Persona, Equipo y Rol no se materializan. No se añade una
relación de Subetapa para Acuerdo o Reunión. La posible relación de Subetapa de
Decisión de Arquitectura se deja a su feature.

La selección inicial del Trámite predeterminado aplica al flujo de creación de
registros funcionales que se defina posteriormente. La especificación permite
que temporalmente no haya Trámite predeterminado y no define el comportamiento
de creación en ese estado; este caso no se implementa aquí.

## Reglas de integridad derivadas de requisitos

- Impedir más de un Trámite activo predeterminado; la aplicación no asigna un
  nombre fijo ni una elección automática.
- Desactivar el Trámite predeterminado puede dejar cero predeterminados.
- Cambiar el predeterminado no modifica el Trámite guardado en registros
  históricos.
- Para una asignación de Subetapa a Hallazgo, comprobar que la Subetapa
  pertenece al mismo Trámite. Ofrecer únicamente Subetapas activas.
- Conservar seguimientos como filas independientes; una actualización del
  registro padre no reemplaza sus seguimientos.
- Minuta: una sola asociada por Reunión; sustitución no crea historial ni
  conserva el archivo reemplazado.
- Toda modificación registra fecha/usuario de creación y modificación, y,
  donde aplique soft delete, indicador activo/equivalente, fecha y usuario de
  eliminación.
- El registro de actividad y los seguimientos deben poder asociarse al usuario
  configurado. No se incorpora autenticación.

## Campos de auditoría

Los nombres lógicos siguientes reflejan la especificación, no fijan el nombre
físico ni el tipo SQL:

- Fecha de creación y usuario de creación.
- Fecha de modificación y usuario de modificación.
- Cuando aplique eliminación lógica: activo/equivalente, fecha de eliminación
  y usuario de eliminación.

La aplicación del soft delete se restringe a entidades funcionales cuya
eliminación esté definida. La especificación no determina cuáles entidades
permiten eliminarse.

## Bloqueadores para esquema integral implementable

1. Definir atributos obligatorios y opcionales para cada entidad funcional.
2. Definir comportamiento de creación cuando no exista Trámite predeterminado.
3. Definir qué entidades admiten soft delete.
4. Definir comportamiento transaccional/compensatorio al fallar el reemplazo
   de archivo de Minuta.
5. Definir atributos y relaciones de Persona, Equipo y Rol o puesto en la
   feature de Equipos y Tecnologías.
6. Definir atributos y ciclo de vida de Decisión de Arquitectura e Informe
   Semanal en sus features respectivas.

Estos puntos quedan pendientes explícitamente; no se resuelven mediante campos
genéricos, relaciones implícitas o cardinalidades supuestas.
