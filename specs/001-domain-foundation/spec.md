# Feature Specification: Fundamentos de Dominio

**Feature Branch**: `001-domain-foundation`

**Created**: 2026-10-09

**Status**: Draft

**Input**: User description: "Crear la feature inicial de fundamentos de dominio para Design Authority, estableciendo una base funcional y conceptual para features posteriores sin implementar pantallas CRUD ni las funcionalidades expresamente excluidas."

## Clarifications

### Session 2026-10-09

- Q: ¿Qué definiciones deben regir el modelo funcional de esta feature? → A: Aplicar únicamente las reglas funcionales confirmadas en esta especificación; dejar las relaciones y reglas no definidas para las features correspondientes.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Establecer el contexto de Trámite (Priority: P1)

Como usuario de Design Authority, necesito que los registros funcionales
pertenezcan al Trámite correspondiente y que los Hallazgos utilicen una
Subetapa de ese mismo Trámite, para mantener el contexto de trabajo sin
extender la relación a entidades para las que no está definida.

**Why this priority**: Trámite y Subetapa son el contexto canónico que
necesitarán las features posteriores.

**Independent Test**: Comprobar que cada Subetapa pertenece a un único Trámite,
que los Hallazgos se asocian a una Subetapa de su Trámite y que, al crear un
registro, se selecciona inicialmente el Trámite predeterminado si existe.

**Acceptance Scenarios**:

1. **Given** que existe un Trámite predeterminado, **When** se inicia la
   creación de un registro funcional, **Then** el Trámite predeterminado aparece
   seleccionado inicialmente.
2. **Given** un Hallazgo asociado a un Trámite, **When** se selecciona su
   Subetapa, **Then** esta pertenece al mismo Trámite.
3. **Given** un registro histórico asociado a un Trámite, **When** cambia el
   Trámite predeterminado, **Then** la asociación histórica no cambia.
4. **Given** un Trámite activo predeterminado, **When** se intenta marcar otro
   Trámite activo también como predeterminado, **Then** el sistema conserva como
   máximo un Trámite activo predeterminado.
5. **Given** que el Trámite predeterminado se administra en los datos,
   **When** se cambia su selección, **Then** no se modifican los Trámites de
   registros históricos.
6. **Given** que se desactiva el Trámite predeterminado, **When** se completa
   la desactivación, **Then** puede quedar temporalmente ningún Trámite
   predeterminado.

---

### User Story 2 - Conservar actividad y seguimiento (Priority: P1)

Como usuario de Design Authority, necesito que la actividad quede asociada al
usuario configurado y que los seguimientos de Hallazgos y Acuerdos se guarden
por separado, para conservar quién hizo cada cambio y el historial de trabajo.

**Why this priority**: La auditoría y el historial de seguimiento son
transversales y no deben perderse al actualizar el estado actual.

**Independent Test**: Registrar un Hallazgo y un Acuerdo con cero seguimientos
y luego agregar múltiples seguimientos a cada uno; verificar que cada entrada
conserve fecha, usuario y observación de forma independiente.

**Acceptance Scenarios**:

1. **Given** una actividad funcional creada o modificada, **When** se consulta
   su auditoría, **Then** están disponibles fecha y usuario de creación y, si
   fue modificada, fecha y usuario de modificación.
2. **Given** un Hallazgo o Acuerdo, **When** se agrega un seguimiento,
   **Then** se conserva como entrada independiente con fecha, usuario y
   observación.
3. **Given** un seguimiento que incluye cambios de estatus o fecha compromiso,
   **When** se guarda, **Then** se pueden consultar los valores anterior y
   nuevo sin sobrescribir las entradas previas.
4. **Given** una entidad funcional que admite eliminación lógica, **When** se
   elimina, **Then** se conservan el indicador de estado y la fecha y el
   usuario de eliminación.
5. **Given** la configuración inicial, **When** se consulta el catálogo de
   tecnologías, **Then** están disponibles Appian, Databricks, Microservicios
   y Power BI.
6. **Given** un Hallazgo, Acuerdo, Decisión de Arquitectura o Reunión,
   **When** se asocia una Tecnología, **Then** el registro queda asociado a una
   sola Tecnología y esa Tecnología puede reutilizarse en otros registros.

---

### User Story 3 - Asociar la minuta vigente a una reunión (Priority: P2)

Como usuario de Design Authority, necesito asociar un archivo de Minuta a una
Reunión y reemplazarlo sin ambigüedad, para identificar el documento vigente
mediante una referencia que no dependa de la ubicación física del servidor.

**Why this priority**: Las Minutas son evidencia asociada a reuniones y deben
tener una regla única de vigencia antes de que otras features las utilicen.

**Independent Test**: Asociar un archivo a una Reunión y reemplazarlo; verificar
que la referencia vigente identifica el archivo más reciente y que no depende
de una ruta absoluta.

**Acceptance Scenarios**:

1. **Given** una Reunión sin Minuta, **When** se asocia un archivo,
   **Then** se conservan su nombre original, nombre físico o identificador
   interno y referencia relativa.
2. **Given** una Reunión con Minuta, **When** se reemplaza por otra,
   **Then** únicamente se conserva el documento más reciente y el anterior no
   se conserva como historial.
3. **Given** una Minuta asociada, **When** se consulta su referencia,
   **Then** esta no contiene una ruta absoluta dependiente del servidor.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: La aplicación MUST usar “Trámite” como único término para el
  concepto de negocio; “Proyecto” y “Proceso” MUST NOT aparecer como conceptos
  distintos en el dominio funcional.
- **FR-002**: El dominio MUST contemplar Trámite, Subetapa, Tecnología, Equipo,
  Persona, Rol, Hallazgo, Seguimiento de Hallazgo, Acuerdo, Seguimiento de
  Acuerdo, Decisión de Arquitectura, Reunión, Minuta e Informe Semanal.
- **FR-003**: Un Trámite MUST poder tener cero o más Subetapas y cada Subetapa
  MUST pertenecer a exactamente un Trámite.
- **FR-004**: Puede existir como máximo un Trámite activo marcado como
  predeterminado. El Trámite predeterminado MUST administrarse manualmente en
  los datos; la aplicación MUST NOT asumir un nombre concreto. Al desactivarlo,
  puede quedar temporalmente ninguno. Cambiarlo MUST NOT modificar registros
  históricos.
- **FR-005**: Si existe un Trámite predeterminado, MUST seleccionarse
  inicialmente al crear un registro funcional. El usuario MUST poder
  seleccionar otro Trámite.
- **FR-006**: Cada Hallazgo MUST pertenecer a un Trámite y utilizar una
  Subetapa de ese mismo Trámite. Una Subetapa MUST pertenecer a un único
  Trámite. Al seleccionar Subetapa para un Hallazgo, solo MUST ofrecerse
  Subetapas activas que pertenezcan al Trámite seleccionado.
- **FR-007**: La aplicación MUST incluir Appian, Databricks, Microservicios y
  Power BI como tecnologías iniciales.
- **FR-008**: En esta etapa la aplicación MUST usar un usuario configurado y
  MUST NOT requerir autenticación. Toda actividad funcional MUST poder
  asociarse a ese usuario.
- **FR-009**: Las entidades modificables MUST permitir consultar fecha y
  usuario de creación y, cuando sean modificadas, fecha y usuario de
  modificación.
- **FR-010**: Cuando una entidad funcional admita eliminación lógica, MUST
  conservar su estado activo o equivalente y fecha y usuario de eliminación.
- **FR-011**: Un Hallazgo MUST poder tener cero o más Seguimientos de Hallazgo;
  cada seguimiento MUST conservarse como registro independiente y permitir
  fecha, usuario y observación.
- **FR-012**: Un Acuerdo MUST poder tener cero o más Seguimientos de Acuerdo;
  cada seguimiento MUST conservarse como registro independiente y permitir
  fecha, usuario y observación.
- **FR-013**: Los seguimientos MUST poder conservar, cuando apliquen, estatus
  anterior y nuevo, y fecha compromiso anterior y nueva, sin sobrescribir los
  seguimientos existentes.
- **FR-014**: Una Reunión puede no tener Minuta o tener una sola Minuta
  asociada.
- **FR-015**: La Minuta MUST conservar el nombre
  original, nombre físico o identificador interno y referencia relativa; MUST
  NOT depender de una ruta absoluta del servidor.
- **FR-016**: Al reemplazar una Minuta, MUST conservarse únicamente el
  documento más reciente. El archivo anterior MUST NOT conservarse como
  historial; esta feature MUST NOT incluir historial de versiones de Minutas.
- **FR-017**: La feature MUST establecer conceptos y reglas base sin
  implementar pantallas CRUD funcionales de Hallazgos, Acuerdos, Decisiones de
  Arquitectura, Reuniones, Minutas o Informes Semanales.
- **FR-018**: La feature MUST NOT incluir importación, exportación ni
  sincronización con Excel, Informes Semanales funcionales, inferencia mediante
  IA, autenticación ni APIs REST.
- **FR-019**: La feature MUST NOT agregar reglas de negocio ni funcionalidades
  que no estén descritas en esta especificación.
- **FR-020**: Hallazgos, Acuerdos, Decisiones de Arquitectura y Reuniones
  MUST pertenecer a una Tecnología. Cada uno de esos registros se asocia a una
  sola Tecnología; una Tecnología puede asociarse a varios registros.
- **FR-021**: La Decisión de Arquitectura puede utilizar una Subetapa; las
  reglas de esa relación MUST definirse en la feature correspondiente. La
  Reunión MUST NOT asociarse a una Subetapa. Esta feature MUST NOT definir una
  asociación de Subetapa para Acuerdos.
- **FR-022**: Persona, Equipo y Rol o puesto MUST contemplarse como conceptos
  del dominio. Las reglas de asignación entre ellos MUST definirse en la
  feature de Equipos y Tecnologías; esta feature MUST NOT establecer
  cardinalidades o asignaciones adicionales.

### Key Entities *(include if feature involves data)*

- **Trámite**: Contexto canónico de negocio; puede tener cero o más Subetapas
  y puede ser administrado manualmente como predeterminado activo. No se
  establece un nombre inicial.
- **Subetapa**: Parte opcional de un Trámite; pertenece a un único Trámite y
  es utilizada por Hallazgos. La Decisión de Arquitectura puede utilizarla
  conforme a las reglas de su feature; su uso por Acuerdos no queda definido
  aquí. La Reunión no se asocia a Subetapa.
- **Tecnología**: Catálogo que incluye inicialmente Appian, Databricks,
  Microservicios y Power BI. Hallazgos, Acuerdos, Decisiones de Arquitectura
  y Reuniones pertenecen a una Tecnología.
- **Equipo, Persona y Rol o puesto**: Conceptos del dominio. Las relaciones y
  reglas de asignación se definirán en la feature de Equipos y Tecnologías.
- **Hallazgo, Acuerdo, Decisión de Arquitectura y Reunión**: Registros
  funcionales asociados a un Trámite y a una Tecnología. El Hallazgo utiliza
  una Subetapa de su Trámite. El uso de Subetapa por la Decisión de
  Arquitectura se definirá en su feature; Reuniones no se asocian a Subetapa y
  esta especificación no establece esa relación para Acuerdos. Hallazgos y
  Acuerdos pueden tener múltiples seguimientos independientes.
- **Seguimiento de Hallazgo y Seguimiento de Acuerdo**: Registro histórico con
  fecha, usuario y observación, y con posibilidad de conservar los cambios
  relevantes especificados.
- **Minuta**: Documento asociado a una Reunión; se conserva únicamente el más
  reciente, con nombre original, nombre físico o identificador interno y
  referencia relativa. No se conserva historial de versiones.
- **Decisión de Arquitectura**: Concepto funcional previsto para una feature
  posterior; esta especificación no define sus flujos CRUD ni reglas completas.
- **Reunión**: Registro que no se asocia a Subetapa y puede tener una Minuta.
- **Informe Semanal**: Concepto previsto para features posteriores; su
  funcionalidad se excluye de esta feature.
- **Usuario configurado**: Identidad inicial asociable a actividades y datos
  de auditoría, sin autenticación en esta etapa.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de los conceptos enumerados en FR-002 aparecen en Key
  Entities y las relaciones confirmadas en esta especificación son coherentes
  entre Requirements, Acceptance Scenarios, Key Entities y Assumptions.
- **SC-002**: El 100% de los Hallazgos de prueba se asocia a una Subetapa que
  pertenece al mismo Trámite que el Hallazgo.
- **SC-003**: En todas las pruebas de cambio de Trámite predeterminado, el 100%
  de los registros históricos conserva su Trámite original.
- **SC-004**: El 100% de los seguimientos de Hallazgo y Acuerdo se conservan
  como entradas independientes con fecha, usuario y observación.
- **SC-005**: En el 100% de las pruebas de reemplazo de Minuta, solo queda el
  documento más reciente, sin conservarse el archivo anterior como historial,
  y la referencia conservada es relativa.
- **SC-006**: La revisión de alcance confirma que no se entregaron pantallas
  CRUD funcionales ni las funcionalidades listadas como fuera de alcance.

## Assumptions

- Esta feature establece el vocabulario, las relaciones y las invariantes
  compartidas; las pantallas operativas y reglas particulares de cada tipo de
  registro se definirán en especificaciones posteriores.
- “Registro funcional” se refiere a un registro de trabajo perteneciente al
  dominio enumerado; no implica que esta feature incluya pantallas CRUD.
- No se define un catálogo de estatus ni una lista obligatoria de atributos
  adicionales para entidades cuyas reglas no estén especificadas.
- La lista de tecnologías indicada constituye el catálogo inicial requerido;
  la administración de ese catálogo no forma parte de esta feature.
- No se asume un nombre para el Trámite predeterminado; este se administra
  manualmente en los datos correspondientes.
- Si no existe un Trámite predeterminado, el comportamiento al crear un
  registro queda pendiente de definición en la feature que requiera ese flujo.
- No se conserva historial de versiones de Minutas; al reemplazarla se
  conserva únicamente el documento más reciente.
- La asociación entre Hallazgo y Subetapa se limita al mismo Trámite. Las
  reglas de Subetapa para Decisión de Arquitectura se definirán en su feature;
  no se define aquí para Acuerdos y se excluye para Reuniones.
- Las relaciones de Persona, Equipo y Rol o puesto se definirán en la feature
  de Equipos y Tecnologías.
- No se agregan cardinalidades o relaciones de Tecnología adicionales a las
  confirmadas en los requisitos funcionales.
