# Feature Specification: Equipos y Tecnologías

**Feature Branch**: Not created (specification only)

**Created**: 2026-10-09

**Status**: Draft

**Input**: User description: "Crear la feature de Equipos y Tecnologías para Design Authority. Definir la estructura funcional para administrar personas, los equipos o tecnologías a los que pertenecen y su rol o puesto. Incluir las tecnologías Appian, Databricks, Microservicios y Power BI; los equipos Arquitectura, Proyecto, Célula y QA; permitir agregar, modificar, listar y eliminar lógicamente personas; mantener auditoría con el usuario configurado; no incluir autenticación, permisos dinámicos, IA, Excel ni APIs REST. No implementar código ni inventar relaciones no definidas."

## Clarifications

### Session 2026-10-09

- Q: ¿Qué regla funcional aplica al Id de Persona? → A: Toda Persona tiene un Id único e inmutable después de su creación; el mecanismo para asignarlo se definirá en el diseño técnico y modelo de datos.
- Q: ¿Cómo se determina y conserva la Fecha de registro? → A: Se asigna automáticamente al crear la Persona y se conserva durante toda su vida; representa la fecha funcional de alta y puede coincidir con FechaCreacion de auditoría, sin exigir dos valores distintos para el mismo evento.
- Q: ¿Qué cardinalidad aplica a los Roles de liderazgo y demás Roles? → A: Cada Rol de liderazgo permite cero o una Persona activa; Desarrollador, Arquitecto, Release manager y Tester permiten cero o más Personas activas.
- Q: ¿Cuántos Equipos o Tecnologías y Roles puede tener una Persona? → A: Exactamente un Equipo o Tecnología y exactamente un Rol o puesto válido para ese Equipo o Tecnología; no se permiten múltiples asignaciones dentro del alcance actual.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consultar el directorio de personas (Priority: P1)

Como usuario de Design Authority, quiero consultar las personas registradas
junto con su Equipo o Tecnología y Rol o puesto, para identificar a los
participantes y sus responsabilidades.

**Why this priority**: El directorio da visibilidad a la estructura funcional
que esta feature establece.

**Independent Test**: Registrar personas de distintos Equipos o Tecnologías y
roles; comprobar que el listado presenta sus datos y asociaciones correctamente.

**Acceptance Scenarios**:

1. **Given** personas registradas, **When** el usuario consulta el directorio,
   **Then** cada fila muestra Id, nombre, apellidos, nombre abreviado, Equipo o
   Tecnología, Rol o puesto y fecha de registro.
2. **Given** una persona eliminada lógicamente, **When** se consulta el
   directorio funcional, **Then** la persona no aparece como activa.
3. **Given** una persona sin apellido materno, **When** aparece en el
   directorio, **Then** sus demás datos se muestran sin exigir ese apellido.
4. **Given** una persona registrada, **When** se modifica, **Then** su Id único
   y su fecha de registro permanecen sin cambios.

---

### User Story 2 - Agregar y modificar una persona (Priority: P1)

Como usuario de Design Authority, quiero agregar y modificar los datos de una
persona y su asignación a un único Equipo o Tecnología y un único Rol o puesto,
para mantener vigente el directorio.

**Why this priority**: La creación y mantenimiento de las personas es la
función principal de administración definida para esta feature.

**Independent Test**: Agregar una persona con cada uno de los campos
obligatorios, modificar sus datos excepto el Id y comprobar el resultado en el
listado.

**Acceptance Scenarios**:

1. **Given** los datos requeridos de una persona, **When** el usuario la agrega,
   **Then** queda registrada con un único Equipo o Tecnología y un único Rol o
   puesto asociado a ese Equipo o Tecnología.
2. **Given** una persona registrada, **When** el usuario modifica sus datos,
   **Then** los cambios se reflejan en el directorio y su Id permanece igual.
3. **Given** un apellido materno no proporcionado, **When** se agrega o modifica
   una persona, **Then** la operación no falla por la ausencia de ese dato.
4. **Given** un nombre, apellido paterno, nombre abreviado, Equipo o Tecnología,
   o Rol o puesto no válido o ausente, **When** se intenta guardar la persona,
   **Then** se informa el error y no se guarda una modificación parcial.
5. **Given** que ya hay varias personas activas en una Tecnología, **When** se
   agrega una persona con otro rol permitido para esa Tecnología, **Then** la
   persona se registra sin impedir la participación de las demás.
6. **Given** que ya existe una persona activa con un rol de liderazgo, **When**
   se intenta asignar el mismo rol de liderazgo a otra persona, **Then** la
   aplicación rechaza la asignación duplicada.
7. **Given** un rol de liderazgo sin titular activo, **When** se consulta o
   administra el directorio, **Then** el rol puede permanecer vacante hasta que
   se asigne una persona.
8. **Given** que una persona es la única titular activa de un rol de liderazgo,
   **When** se elimina lógicamente, **Then** el rol queda vacante y puede
   asignarse posteriormente a otra persona.

---

### User Story 3 - Retirar una persona del directorio (Priority: P2)

Como usuario de Design Authority, quiero eliminar lógicamente una persona,
para retirarla del directorio activo sin perder su trazabilidad.

**Why this priority**: La lista activa debe reflejar a los participantes
vigentes y preservar auditoría de las operaciones.

**Independent Test**: Eliminar lógicamente una persona y verificar que deja de
aparecer como activa y que se registran los datos de eliminación.

**Acceptance Scenarios**:

1. **Given** una persona activa, **When** el usuario la elimina, **Then** queda
   inactiva y se conservan la fecha y el usuario de eliminación.
2. **Given** una persona eliminada, **When** se consultan sus datos de
   auditoría, **Then** se conserva la fecha y el usuario de creación y la
   modificación más reciente, si existió.

### Edge Cases

- El apellido materno puede estar vacío.
- No debe permitirse cambiar el Id durante una modificación.
- No se define aquí si los nombres de personas deben ser únicos.
- Un rol de liderazgo puede quedar vacante, pero no puede tener más de una
  persona activa asignada.
- Una persona eliminada lógicamente no debe presentarse como activa.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: La aplicación MUST mostrar las personas registradas y para cada
  una MUST presentar Id, nombre, apellido paterno, apellido materno cuando
  exista, nombre abreviado, Equipo o Tecnología, Rol o puesto y fecha de
  registro.
- **FR-002**: La aplicación MUST permitir agregar, modificar y eliminar
  lógicamente una persona.
- **FR-003**: Cada Persona MUST tener un Id único e inmutable después de su
  creación. Esta especificación funcional MUST NOT definir el mecanismo técnico
  para asignar el Id; esa estrategia se definirá durante el diseño técnico y
  del modelo de datos.
- **FR-004**: Cada persona MUST tener nombre, apellido paterno, nombre
  abreviado, una Tecnología o Equipo y un Rol o puesto. El apellido materno
  MUST ser opcional.
- **FR-005**: Cada persona MUST pertenecer a exactamente una de estas
  Tecnologías o Equipos dentro del alcance actual:
  Appian, Databricks, Microservicios, Power BI, Arquitectura, Proyecto, Célula
  o QA.
- **FR-006**: Cada persona MUST tener exactamente un Rol o puesto asociado a
  su única Tecnología o Equipo. Esta feature MUST NOT permitir múltiples Roles
  por Persona.
- **FR-007**: Los roles iniciales disponibles MUST corresponder a la
  Tecnología o Equipo seleccionado:
  - Appian, Databricks, Microservicios y Power BI: Líder técnico y
    Desarrollador.
  - Arquitectura: Líder de arquitectura y Arquitecto.
  - Proyecto: Líder de proyecto y Release manager.
  - Célula: Líder de célula.
  - QA: Líder de QA y Tester.
- **FR-008**: Los Roles Líder técnico de Appian, Líder técnico de Databricks,
  Líder técnico de Microservicios, Líder técnico de Power BI, Líder de
  arquitectura, Líder de proyecto, Líder de célula y Líder de QA MUST permitir
  cero o una Persona activa asignada. Los Roles Desarrollador, Arquitecto,
  Release manager y Tester MUST permitir cero o más Personas activas.
- **FR-009**: Al modificar una persona, la aplicación MUST conservar su Id y
  MUST permitir actualizar los demás datos definidos para la persona,
  incluyendo su Equipo o Tecnología y Rol o puesto.
- **FR-010**: La Fecha de registro MUST asignarse automáticamente al crear una
  Persona y MUST conservarse durante toda la vida del registro. Representa la
  fecha funcional de alta y puede coincidir con FechaCreacion de auditoría;
  MUST NOT exigirse mantener dos valores distintos cuando representan el
  mismo evento.
- **FR-011**: La aplicación MUST validar en servidor los datos de la persona
  antes de guardarlos e MUST informar los errores de validación sin guardar
  cambios parciales.
- **FR-012**: La eliminación de persona MUST ser lógica; debe conservar un
  indicador activo o equivalente, fecha de eliminación y usuario de
  eliminación. Una persona eliminada MUST dejar de mostrarse como activa.
- **FR-013**: Las operaciones de creación y modificación MUST conservar fecha
  y usuario de creación y fecha y usuario de modificación, según corresponda.
- **FR-014**: La auditoría MUST continuar asociándose al usuario configurado
  existente en los fundamentos de dominio. Esta feature MUST NOT implementar
  autenticación.
- **FR-015**: La presentación del directorio MUST utilizar las columnas
  definidas para esta feature sin requerir personalización de columnas.
- **FR-016**: Esta feature MUST NOT incluir administración dinámica de
  permisos, roles de seguridad, autorización, IA, importación o exportación de
  Excel ni APIs REST.
- **FR-017**: Esta feature MUST NOT crear relaciones adicionales entre
  Persona, Equipo o Tecnología y Rol o puesto distintas de la asignación única
  especificada aquí.
- **FR-018**: Esta feature MUST definir la información y operaciones descritas
  sin implementar código.

### Key Entities *(include if feature involves data)*

- **Persona**: Participante del dominio con Id inmutable, nombre, apellido
  paterno, apellido materno opcional, nombre abreviado, una Tecnología o
  Equipo, un Rol o puesto y fecha de registro; conserva auditoría y estado de
  eliminación lógica.
- **Tecnología o Equipo**: Una de las ocho opciones iniciales definidas en
  FR-005. No se define aquí mantenimiento dinámico de ese conjunto.
- **Rol o puesto**: Una de las opciones iniciales asociadas al Equipo o
  Tecnología según FR-007. Cada Persona tiene un único Rol o puesto en esta
  feature.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: En todas las pruebas de alta y modificación, el 100% de las
  personas conserva un Id único e inmutable, exactamente un Equipo o
  Tecnología y exactamente un Rol o puesto válido para esa asignación.
- **SC-002**: El 100% de los registros listados muestra los campos definidos en
  FR-001, respetando que apellido materno sea opcional.
- **SC-003**: En el 100% de las pruebas de eliminación lógica, la persona deja
  de aparecer como activa y su fecha y usuario de eliminación siguen
  disponibles para auditoría.
- **SC-004**: El 100% de las altas y modificaciones exitosas conserva los datos
  de auditoría requeridos y asocia la actividad al usuario configurado.
- **SC-005**: La revisión de la feature confirma que están disponibles las
  ocho Tecnologías o Equipos y las asignaciones de Rol o puesto indicadas en
  FR-007.
- **SC-006**: En el 100% de las pruebas, cada Rol de liderazgo tiene cero o
  una Persona activa y cada Rol adicional permite cero o más Personas activas.
- **SC-007**: En el 100% de las pruebas de alta y modificación, la Fecha de
  registro se asigna al crear la Persona y permanece sin cambios durante toda
  la vida del registro; no se exige duplicarla si coincide con FechaCreacion de
  auditoría.

## Assumptions

- Cada Persona se asigna exactamente a una de las ocho opciones iniciales y a
  un único Rol o puesto válido para esa opción; no se agregan asignaciones
  secundarias.
- La lista inicial de Tecnologías, Equipos y Roles es la indicada en esta
  especificación. Su administración dinámica no está incluida.
- El Id de Persona es único e inmutable después de la creación. Su mecanismo
  técnico de asignación queda para el diseño técnico y del modelo de datos.
- El nombre, apellido paterno, nombre abreviado, Equipo o Tecnología, Rol o
  puesto e Id son obligatorios porque la solicitud los presenta como datos de
  cada persona; apellido materno es opcional.
- La Fecha de registro se asigna automáticamente al crear a la Persona y se
  conserva durante toda su vida. Puede coincidir con FechaCreacion de auditoría
  y no se requieren dos valores distintos para el mismo evento.
- Cada rol de liderazgo puede quedar temporalmente sin titular activo, pero no
  puede tener más de un titular activo. Los roles adicionales permiten varios
  participantes.
- Ser titular de un rol de liderazgo no excluye que otras personas puedan ser
  responsables de Hallazgos o Acuerdos; la asignación de responsabilidades en
  esos registros se definirá en sus features.
- No se establecen reglas de unicidad para nombres ni criterios para
  restaurar una persona eliminada lógicamente.
- La auditoría usa el usuario configurado descrito en
  [001-domain-foundation](../001-domain-foundation/spec.md); no se añade
  autenticación.
- La especificación describe necesidades funcionales, no autoriza iniciar
  implementación. La aprobación del plan y de las tareas y la definición
  integral del esquema inicial siguen siendo prerrequisitos constitucionales.
