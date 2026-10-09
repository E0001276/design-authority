# Feature Specification: MVP Operativo de Design Authority

**Feature Branch**: `003-design-authority-mvp`

**Created**: 2026-10-09

**Status**: Draft

**Input**: User description: "Construir la funcionalidad operativa principal del MVP de Design Authority para Trámites, Subetapas, Hallazgos, seguimientos, Acuerdos, seguimientos, Decisiones de Arquitectura, Reuniones y Minutas; basarse en la documentación funcional original, la Constitution y las features 001 y 002; excluir Informe Semanal, Excel, IA, autenticación, autorización y APIs REST; no inventar reglas y dejar las ambigüedades para aclaración."

## Clarifications

### Session 2026-10-09

- Q: ¿Qué ocurre al crear un registro si no hay Trámite predeterminado? → A:
  El usuario selecciona manualmente un Trámite activo.
- Q: ¿Cómo se relaciona la Subetapa con una Decisión de Arquitectura? → A:
  Cada Decisión pertenece a una Subetapa del mismo Trámite.
- Q: ¿Qué formato y tamaño máximo se permiten para una Minuta? → A: Solo
  archivos `.docx` de hasta 5 MB.
- Q: ¿Los estatus de Hallazgos, Acuerdos y Decisiones pueden cambiarse
  libremente entre sus valores permitidos, o deben seguir secuencias
  específicas? → A: Se pueden cambiar libremente entre los valores permitidos.
- Q: ¿En una Reunión se permite que la lista de asistentes o la de personas
  que no asistieron quede vacía? → A: Debe haber al menos un asistente; la
  lista de ausentes puede quedar vacía.
- Q: ¿Qué estatus deben aparecer en los listados iniciales de Hallazgos y
  Acuerdos descritos como “pendientes”? → A: Solo Abierto y En progreso.
- Q: ¿El consecutivo XXX de las Decisiones de Arquitectura se reinicia por
  Trámite o Subetapa? → A: Es global para toda la aplicación y no se reinicia
  por Trámite ni por Subetapa.
- Q: ¿Se aprueba el modelo consolidado de `data-model.md` como esquema inicial
  del MVP? → A: Sí. El modelo de las features 001–003 queda aprobado como
  esquema inicial; Informe Semanal y Excel se incorporarán mediante
  migraciones posteriores.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar y dar seguimiento a Hallazgos (Priority: P1)

Como usuario de Design Authority, quiero registrar Hallazgos asociados al
Trámite, Subetapa y Tecnología correspondientes y conservar sus seguimientos,
para dar trazabilidad a las observaciones identificadas durante reuniones o
revisiones.

**Why this priority**: Los Hallazgos y sus seguimientos son parte central del
registro operativo descrito en la documentación funcional.

**Independent Test**: Con los catálogos requeridos disponibles, crear un
Hallazgo, consultarlo en el listado, modificarlo, registrar más de un
seguimiento y comprobar que cada seguimiento se conserva por separado.

**Acceptance Scenarios**:

1. **Given** los catálogos de Trámite, Subetapa, Tecnología y Persona
   disponibles, **When** se crea un Hallazgo con los campos requeridos,
   **Then** queda asociado a un Trámite, una Subetapa activa de ese mismo
   Trámite, una Tecnología y una Persona responsable, e inicia con estatus
   Abierto.
2. **Given** un Hallazgo existente, **When** se modifica, **Then** se
   actualizan sus datos sin cambiar su Id y se conservan los datos de auditoría
   aplicables.
3. **Given** un Hallazgo, **When** se registran varios seguimientos,
   **Then** cada uno conserva su fecha, usuario y observación como una entrada
   independiente y, cuando cambian, los estatus y fechas compromiso anterior y
   nuevos.
4. **Given** Hallazgos en el listado, **When** el usuario los consulta, busca por
   descripción, filtra por Tecnología o estatus, ordena por los campos
   admitidos o cambia de página, **Then** la lista refleja esos criterios y
   mantiene visibles Id, Tecnología y descripción.
5. **Given** un Hallazgo activo, **When** se elimina lógicamente, **Then** deja
   de aparecer en el listado activo, permanece disponible para trazabilidad y
   conserva sus seguimientos.
6. **Given** que el usuario intenta guardar un Hallazgo con un dato obligatorio
   ausente o con una Subetapa de otro Trámite, **When** se valida la operación,
   **Then** se informa el error y no se guarda una modificación parcial.

---

### User Story 2 - Registrar y dar seguimiento a Acuerdos (Priority: P1)

Como usuario de Design Authority, quiero registrar Acuerdos con su Tecnología
y Persona responsable y conservar el historial de sus seguimientos, para
consultar sus compromisos y evolución sin sobrescribir observaciones previas.

**Why this priority**: Los Acuerdos y su historial son el otro flujo principal
de seguimiento del MVP.

**Independent Test**: Crear y modificar un Acuerdo, agregar varios
seguimientos, comprobar su consulta y verificar la eliminación lógica sin
pérdida del historial.

**Acceptance Scenarios**:

1. **Given** los catálogos de Trámite, Tecnología y Persona disponibles,
   **When** se crea un Acuerdo con sus campos requeridos, **Then** queda
   asociado a un Trámite, una Tecnología y una Persona responsable, e inicia
   con estatus Abierto.
2. **Given** un Acuerdo existente, **When** se modifica, **Then** sus datos se
   actualizan sin cambiar su Id y se conserva la auditoría aplicable.
3. **Given** un Acuerdo, **When** se agregan varios seguimientos, **Then**
   cada seguimiento permanece como registro independiente con fecha, usuario y
   observación, y con los valores anterior y nuevo de estatus o fecha
   compromiso cuando haya cambios.
4. **Given** Acuerdos en el listado, **When** el usuario los consulta, busca por
   descripción, filtra por Tecnología o estatus, ordena por los campos
   admitidos o cambia de página, **Then** la lista refleja esos criterios y
   mantiene visibles Id, Tecnología y descripción.
5. **Given** un Acuerdo activo, **When** se elimina lógicamente, **Then** deja
   de aparecer en el listado activo, permanece disponible para trazabilidad y
   conserva sus seguimientos.
6. **Given** que el usuario intenta guardar un Acuerdo con un dato obligatorio
   ausente, **When** se valida la operación, **Then** se informa el error y no
   se guarda una modificación parcial.

---

### User Story 3 - Registrar Decisiones de Arquitectura (Priority: P2)

Como usuario de Design Authority, quiero registrar y consultar Decisiones de
Arquitectura relacionadas con un Trámite y una Tecnología, para conservar la
decisión, su contexto y el equipo responsable.

**Why this priority**: Las Decisiones documentan el resultado de las revisiones
de arquitectura y complementan los registros de Hallazgos y Acuerdos.

**Independent Test**: Crear una Decisión con su identificador, campos
requeridos, Trámite, Tecnología y equipo responsable; consultarla, modificarla,
buscarla y retirarla lógicamente.

**Acceptance Scenarios**:

1. **Given** los catálogos requeridos disponibles, **When** se registra una
   Decisión con Id, Tecnología, descripción del problema, enfoque de solución,
   decisión, equipo responsable, fecha de registro y estatus, **Then** sus
   datos quedan disponibles en el listado.
2. **Given** una Decisión existente, **When** se modifica, **Then** se
   actualizan sus datos sin cambiar el Id.
3. **Given** Decisiones registradas, **When** el usuario busca por descripción,
   filtra por Tecnología o estatus, ordena por fecha de registro o cambia de
   página, **Then** la lista refleja los criterios y mantiene visibles Id,
   Tecnología y descripción del problema.
4. **Given** una Decisión activa, **When** se elimina lógicamente, **Then**
   deja de aparecer en el listado activo y conserva la auditoría aplicable.
5. **Given** Decisiones asociadas a distintos Trámites o Subetapas,
   **When** se registran en secuencia, **Then** cada Id respeta la nomenclatura
   `[Trámite]-[Subetapa]-XXX` y `XXX` continúa el consecutivo global sin
   reiniciarse por Trámite ni por Subetapa; cada Decisión pertenece a una
   Subetapa del mismo Trámite.

---

### User Story 4 - Registrar Reuniones y su Minuta vigente (Priority: P2)

Como usuario de Design Authority, quiero registrar Reuniones de Diseño
Técnico, sus participantes y una Minuta vigente, para consultar la actividad y
el documento asociado sin depender de rutas absolutas ni conservar versiones
anteriores.

**Why this priority**: Las reuniones y minutas documentan el contexto de las
actividades que generan Hallazgos, Acuerdos y Decisiones.

**Independent Test**: Registrar una Reunión con los campos requeridos y una
Minuta, consultarla, reemplazar la Minuta y verificar que solo permanezca
vigente el documento más reciente.

**Acceptance Scenarios**:

1. **Given** los catálogos de Trámite, Tecnología y Persona disponibles,
   **When** se registra una Reunión con los datos requeridos, **Then** queda
   asociada a un Trámite y una Tecnología y conserva fecha, hora, estatus,
   al menos una persona asistente, cero o más personas que no asistieron,
   resumen de Acuerdos, resumen de compromisos y asunto; no se asocia a
   Subetapa.
2. **Given** una Reunión registrada, **When** se modifica o se elimina
   lógicamente, **Then** el Id no cambia y, al eliminarla, deja de aparecer en
   el listado activo conservando su auditoría aplicable.
3. **Given** una Reunión sin Minuta vigente, **When** se le asocia un archivo,
   **Then** se conservan el nombre original, el nombre físico o identificador y
   una referencia relativa al archivo.
4. **Given** una Reunión con Minuta vigente, **When** se reemplaza el
   documento, **Then** solamente el documento nuevo permanece como vigente y
   no se conserva historial de versiones.
5. **Given** Reuniones registradas, **When** el usuario busca por asunto,
   ordena por Id, Tecnología, asunto o fecha, o cambia de página, **Then** la
   lista refleja esos criterios y mantiene visibles Id, Tecnología y asunto.
6. **Given** que el usuario intenta guardar datos obligatorios ausentes o un
   archivo que no cumple las reglas definidas, **When** se valida la
   operación, **Then** se informa el error y no se guarda una modificación
   parcial.
7. **Given** una Persona seleccionada como asistente de una Reunión, **When** se intenta incluir también como ausente, **Then** la aplicación rechaza la duplicidad y no guarda cambios parciales.

### Edge Cases

- Puede no existir temporalmente un Trámite activo predeterminado; en ese caso,
  el usuario elige manualmente un Trámite activo al crear un registro.
- Solo deben ofrecerse para Hallazgos Subetapas activas del Trámite
  seleccionado; una Subetapa de otro Trámite no es válida.
- No se define asociación de Reunión con Subetapa.
- Una Decisión de Arquitectura pertenece a una Subetapa del mismo Trámite.
- Una Reunión puede tener cero o una Minuta vigente; reemplazarla no crea
  historial.
- Una misma Persona no puede aparecer simultáneamente en la lista de
  asistentes y en la lista de personas que no asistieron de una misma Reunión.  
- Los registros con eliminación lógica no deben aparecer en listados activos
  ni provocar la pérdida de seguimientos de Hallazgos o Acuerdos.
- La carga de Minutas acepta archivos `.docx` de hasta 5 MB.

## Requirements *(mandatory)*

### Functional Requirements

#### Trámite, Subetapa y Tecnología

- **FR-001**: La aplicación MUST usar “Trámite” como único término para el
  concepto de negocio; MUST NOT crear conceptos distintos denominados Proyecto
  o Proceso.
- **FR-002**: Los catálogos de Trámites y Subetapas MUST ser los definidos en
  `001-domain-foundation` y MUST administrarse manualmente en los datos; esta
  feature MUST NOT agregar pantallas de administración para dichos catálogos.
  Trámite MUST contemplar Id, nombre, descripción opcional, indicador
  predeterminado y estado activo. Subetapa MUST contemplar Id, nombre,
  descripción opcional, estado activo y el Trámite al que pertenece.
- **FR-003**: Puede existir como máximo un Trámite activo marcado como
  predeterminado. El nombre del predeterminado no se fija en esta
  especificación. Si existe, MUST seleccionarse inicialmente al crear un
  registro funcional y el usuario MUST poder seleccionar otro Trámite. Cambiar
  el predeterminado MUST NOT modificar registros históricos. Desactivarlo
  puede dejar temporalmente ninguno.
- **FR-004**: Cuando no existe un Trámite predeterminado, el usuario MUST
  seleccionar manualmente un Trámite activo para crear un registro funcional.
- **FR-005**: La aplicación MUST utilizar las Tecnologías iniciales Appian,
  Databricks, Microservicios y Power BI. Hallazgos, Acuerdos, Decisiones de
  Arquitectura y Reuniones MUST asociarse a una Tecnología y MUST NOT aceptar
  como tal los Equipos Arquitectura, Proyecto, Célula o QA definidos en
  `002-equipos-tecnologias`.
- **FR-006**: Cada Hallazgo MUST pertenecer a un Trámite y a una Subetapa
  activa de ese mismo Trámite. Al seleccionar Subetapa para un Hallazgo, solo
  MUST ofrecerse Subetapas activas del Trámite seleccionado.
- **FR-007**: Esta especificación MUST NOT asociar Reuniones con Subetapa ni
  asociar Acuerdos con Subetapa. Cada Decisión de Arquitectura MUST pertenecer
  a una Subetapa del mismo Trámite.

#### Hallazgos y Seguimientos de Hallazgos

- **FR-008**: La aplicación MUST permitir crear, consultar, modificar y
  eliminar lógicamente Hallazgos.
- **FR-009**: Cada Hallazgo MUST requerir Id único e inmutable, fecha de
  registro, Tecnología, Subetapa, categoría, descripción, una Persona
  responsable, estatus, criticidad, prioridad, fase de registro, fase de
  atención y fecha compromiso. El Id MUST ser consecutivo, iniciar en 1 y
  aumentar de uno en uno. La fecha de solución MUST ser opcional.
- **FR-010**: Las categorías iniciales de Hallazgo MUST ser Documental,
  Convenciones, Lineamientos y estándares, Reutilización, Diseños, Uso de
  Patrones y Buenas prácticas.
- **FR-011**: Los estatus iniciales de Hallazgo MUST ser Abierto, En progreso,
  Completado, Cancelado, Bloqueado y No entregó. Los Hallazgos nuevos MUST
  iniciar con estatus Abierto.
- **FR-012**: Las criticidades MUST ser No aplica, Crítico, Mayor, Menor y
  Sugerencia. Las prioridades MUST ser Alta, Media y Baja.
- **FR-013**: La fase de registro MUST admitir No aplica, Antes del
  desarrollo, Durante el desarrollo, Después del desarrollo y No entregó. La
  fase de atención MUST admitir Diseño, Construcción, Pruebas, Certificación,
  Soporte, Mantenimiento, En validación y No aplica.
- **FR-014**: Cada Hallazgo MUST poder tener cero o más Seguimientos de
  Hallazgo. Cada seguimiento MUST conservarse como registro independiente con
  fecha, usuario y observación, y MUST permitir conservar estatus anterior y
  nuevo y fecha compromiso anterior y nueva cuando correspondan.
- **FR-015**: El seguimiento más reciente de un Hallazgo MUST NOT sobrescribir
  seguimientos anteriores. Los campos heredados de fecha de actualización y
  observación MUST representarse mediante los seguimientos independientes.

#### Acuerdos y Seguimientos de Acuerdos

- **FR-016**: La aplicación MUST permitir crear, consultar, modificar y
  eliminar lógicamente Acuerdos.
- **FR-017**: Cada Acuerdo MUST requerir Id único e inmutable, consecutivo,
  iniciado en 1 y aumentado de uno en uno, fecha de
  registro, Trámite, Tecnología, descripción, una Persona responsable,
  estatus y fecha compromiso. La fecha de solución MUST ser opcional.
- **FR-018**: Los estatus iniciales de Acuerdo MUST ser Abierto, En progreso,
  Completado, Cancelado, Bloqueado y No entregó. Los Acuerdos nuevos MUST
  iniciar con estatus Abierto.
- **FR-019**: Cada Acuerdo MUST poder tener cero o más Seguimientos de Acuerdo.
  Cada seguimiento MUST conservarse como registro independiente con fecha,
  usuario y observación, y MUST permitir conservar estatus anterior y nuevo y
  fecha compromiso anterior y nueva cuando correspondan.
- **FR-020**: El seguimiento más reciente de un Acuerdo MUST NOT sobrescribir
  seguimientos anteriores. Los campos heredados de fecha de actualización y
  observación MUST representarse mediante los seguimientos independientes.

#### Decisiones de Arquitectura

- **FR-021**: La aplicación MUST permitir crear, consultar, modificar y
  eliminar lógicamente Decisiones de Arquitectura.
- **FR-022**: Cada Decisión MUST requerir Id único e inmutable, Trámite,
  Tecnología, descripción del problema enfrentado, enfoque de solución,
  decisión, un Equipo responsable, fecha de registro y estatus.
- **FR-023**: El Id de Decisión MUST seguir la nomenclatura funcional
  `[Trámite]-[Subetapa]-XXX`, donde `XXX` es un consecutivo global para toda
  la aplicación que MUST NOT reiniciarse por Trámite ni por Subetapa. Cada
  Decisión MUST pertenecer a una Subetapa del mismo Trámite.
- **FR-024**: Los estatus de Decisión MUST ser Abierto, En progreso, Cancelado
  y Cerrado.

#### Reuniones y Minutas

- **FR-025**: La aplicación MUST permitir crear, consultar, modificar y
  eliminar lógicamente Reuniones.
- **FR-026**: Cada Reunión MUST pertenecer a un Trámite y una Tecnología y
  MUST NOT pertenecer a una Subetapa. MUST requerir Id único e inmutable,
  fecha, hora, estatus, al menos una persona asistente, una lista de personas
  que no asistieron que puede estar vacía, resumen de Acuerdos, resumen de
  compromisos y asunto. Comentarios MUST ser opcionales.
- **FR-027**: Los estatus de Reunión MUST ser Realizada, Equipo no asistió a
  la reunión, Reprogramada y Cancelada.
- **FR-028**: Una Reunión MUST poder tener cero o una Minuta vigente. La Minuta
  vigente MUST conservar nombre original, nombre físico o identificador
  interno y referencia relativa al archivo. MUST NOT guardarse una ruta
  absoluta dependiente del servidor.
- **FR-029**: Al reemplazar una Minuta, MUST conservarse únicamente el
  documento más reciente. El archivo anterior MUST NOT conservarse como
  versión histórica.
- **FR-030**: La Minuta MUST ser un archivo `.docx` de hasta 5 MB. La
  aplicación MUST rechazar archivos con otra extensión o que excedan ese
  tamaño.

#### Consulta, auditoría y validación

- **FR-031**: Los listados MUST permitir paginación. Las vistas iniciales de
  Hallazgos y Acuerdos MUST incluir únicamente registros con estatus Abierto o
  En progreso. Hallazgos y Acuerdos MUST permitir búsqueda por descripción,
  filtros por Tecnología y estatus, y ordenamiento por Id, Tecnología,
  estatus o fecha de registro. Decisiones MUST permitir búsqueda por
  descripción, filtros por Tecnología y estatus, y ordenamiento por fecha de
  registro. Reuniones MUST permitir búsqueda por asunto y ordenamiento por Id,
  Tecnología, asunto o fecha.
- **FR-032**: La personalización de columnas MUST estar disponible para
  Hallazgos, Acuerdos, Decisiones de Arquitectura y Reuniones. En sus listados
  MUST permanecer visible Id, Tecnología y la columna descriptiva principal
  correspondiente: descripción de Hallazgo, descripción del problema,
  descripción de Acuerdo o asunto de Reunión.
- **FR-033**: La aplicación MUST conservar auditoría de creación y
  modificación mediante fecha y usuario configurado. Las entidades retiradas
  lógicamente MUST conservar estado, fecha y usuario de eliminación. No se
  implementará autenticación.
- **FR-034**: Los registros funcionales eliminados lógicamente MUST excluirse
  de listados activos normales y MUST conservar los seguimientos de Hallazgos
  y Acuerdos.
- **FR-035**: Toda entrada MUST validarse en servidor. Un error de validación
  MUST impedir guardar cambios parciales.
- **FR-036**: La aplicación MUST NOT incluir Informe Semanal, importación,
  exportación o sincronización de Excel, inferencia mediante IA, autenticación,
  autorización ni APIs REST en esta feature.
- **FR-037**: Esta especificación MUST NOT autorizar la implementación de
  código ni scripts ejecutables; su implementación requiere plan y tareas
  aprobadas y cumplimiento de la Constitution. El modelo consolidado de
  `data-model.md` para las features 001–003 queda aprobado como esquema inicial
  del MVP y puede utilizarse como base para generar scripts versionados y
  código durante la implementación. Informe Semanal y Excel se incorporarán
  mediante migraciones posteriores.
- **FR-038**: Al crear o modificar Hallazgos, Acuerdos y Decisiones, el usuario
  MUST poder seleccionar cualquier estatus definido para el tipo de registro;
  no se requieren secuencias específicas de transición.
- **FR-039**: Cada Reunión MUST incluir al menos una Persona asistente. La
  lista de personas que no asistieron MUST poder quedar vacía.
- **FR-039A**: Una Persona MUST NOT aparecer simultáneamente como asistente
  y como ausente en una misma Reunión. Cada Persona participante de una
  Reunión MUST tener un único estado de asistencia para esa Reunión.  
- **FR-040**: Las reuniones MUST tener un Id numérico único e inmutable,
  consecutivo, iniciado en 1 y aumentado de uno en uno.
- **FR-041**: La aplicación MUST registrar los errores y eventos operativos
  relevantes para diagnóstico y MUST NOT mostrar detalles técnicos sensibles
  al usuario cuando una operación falla.

### Key Entities *(include if feature involves data)*

- **Trámite**: Catálogo manual que representa el contexto canónico para los
  registros funcionales; puede existir como máximo uno activo predeterminado.
- **Subetapa**: Catálogo manual que pertenece a un único Trámite. Hallazgos
  utilizan una Subetapa activa de su mismo Trámite.
- **Tecnología**: Una de Appian, Databricks, Microservicios o Power BI,
  distinta semánticamente de los Equipos.
- **Hallazgo**: Registro con contexto de Trámite, Subetapa y Tecnología,
  responsable Persona, datos de categorización y fechas, y cero o más
  seguimientos.
- **Seguimiento de Hallazgo**: Entrada histórica independiente con fecha,
  usuario, observación y cambios relevantes cuando apliquen.
- **Acuerdo**: Compromiso asociado a Trámite, Tecnología y Persona responsable,
  con cero o más seguimientos; no se asocia a Subetapa.
- **Seguimiento de Acuerdo**: Entrada histórica independiente con fecha,
  usuario, observación y cambios relevantes cuando apliquen.
- **Decisión de Arquitectura**: Registro de una decisión con Tecnología,
  Equipo responsable, una Subetapa del mismo Trámite, un Id cuyo consecutivo
  es global para toda la aplicación y los datos descritos.
- **Reunión**: Sesión de Diseño Técnico asociada a Trámite y Tecnología, con
  fecha, hora, estatus, asistentes, ausentes y resúmenes; no se asocia a
  Subetapa.
- **Minuta**: Documento vinculado a una Reunión; puede no existir o existir
  una vigente. Al reemplazarla, solo queda el documento más reciente.
- **Persona**: Entidad reutilizada de `002-equipos-tecnologias` para
  responsables y participantes; no se crea un catálogo duplicado.
- **Equipo**: Entidad reutilizada de `002-equipos-tecnologias`, incluyendo el
  Equipo responsable de una Decisión cuando corresponda.
- **Usuario configurado**: Identidad inicial de Design Authority para
  auditoría y actividad; no requiere autenticación.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de los Hallazgos de prueba se asocia a una Tecnología y a
  una Subetapa activa del mismo Trámite; se rechaza cualquier combinación de
  Trámite y Subetapa no coincidente.
- **SC-002**: El 100% de los seguimientos de Hallazgos y Acuerdos se conserva
  como entradas independientes con fecha, usuario y observación, sin
  sobrescribir entradas previas.
- **SC-003**: El 100% de Hallazgos, Acuerdos, Decisiones y Reuniones de prueba
  puede consultarse, modificarse y retirarse lógicamente según su flujo; los
  registros retirados no aparecen en listados activos.
- **SC-004**: En todas las pruebas de Minuta, solo se acepta `.docx` de hasta
  5 MB; en las sustituciones solo se conserva vigente el documento más
  reciente y la referencia guardada es relativa.
- **SC-005**: El 100% de las altas y modificaciones válidas conserva la
  auditoría con el usuario configurado; las operaciones inválidas no guardan
  modificaciones parciales.
- **SC-006**: Los usuarios pueden localizar un registro del listado mediante
  los campos de búsqueda definidos y reducir los resultados mediante los
  filtros definidos para cada módulo.
- **SC-007**: La revisión funcional confirma que la feature no incluye Informe
  Semanal, Excel, IA, autenticación, autorización ni APIs REST.
- **SC-008**: El 100% de las altas o modificaciones válidas de Hallazgos,
  Acuerdos y Decisiones puede guardar cualquiera de los estatus definidos, sin
  requerir una secuencia específica. El 100% de las Reuniones válidas conserva
  al menos una Persona asistente.
- **SC-009**: El 100% de los errores de operación probados presenta al usuario
  un mensaje sin detalles técnicos sensibles y conserva información relevante
  para diagnóstico.
- **SC-010**: En una secuencia de creación de Decisiones asociadas a distintos
  Trámites o Subetapas, el sufijo consecutivo aumenta globalmente y no se
  reinicia al cambiar el Trámite o la Subetapa.

## Assumptions

- Los catálogos de Trámites y Subetapas se administran manualmente en los
  datos; esta feature los consume y no agrega pantallas para administrarlos.
- Los catálogos de Tecnología, Equipo y Persona se reutilizan según
  `002-equipos-tecnologias`; las entidades funcionales descritas solo admiten
  Tecnología, no elementos clasificados como Equipo.
- Una Decisión de Arquitectura utiliza una Subetapa perteneciente al mismo
  Trámite, conforme a la aclaración registrada en esta especificación.
- Hallazgos, Acuerdos, Decisiones y Reuniones pueden consultarse y modificarse
  por la aplicación, pero no se cambia el Trámite guardado en registros
  históricos por modificar el valor predeterminado.
- El usuario configurado y la auditoría se rigen por
  `001-domain-foundation`; no se incorpora autenticación.
- No se conserva historial de versiones de Minutas. La regla de reemplazo de
  archivo no define retención del archivo anterior fuera de la aplicación.
- Los valores iniciales de categorías, estatus, criticidad, prioridad y fases
  son los documentados en la fuente funcional y enumerados en Requirements.
- El modelo consolidado de `data-model.md` para las features 001–003 está
  aprobado como esquema inicial del MVP. Informe Semanal y Excel quedan fuera
  de este esquema y se incorporarán mediante migraciones posteriores.
- Esta aplicación personal no requiere alta disponibilidad, concurrencia
  elevada ni niveles de servicio estrictos.
