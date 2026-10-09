<!--
Sync Impact Report
Version change: uninitialized scaffold -> 1.0.0
Modified principles: five unresolved template slots -> the 20 principles supplied
  for Design Authority; no previously adopted principle was available to preserve:
  Spec-Driven Development; Arquitectura en Capas; Stack Tecnológico Obligatorio;
  Sin Entity Framework; Sin APIs REST Internas por Defecto; Repositorios e
  Inyección de Dependencias; Protección de Secretos y Validación; Eliminación
  Lógica y Auditoría; Scripts de Base de Datos Versionados; Diseño Integral del
  Esquema Inicial; Usuario Inicial Configurado; Terminología Canónica del
  Dominio; Relación entre Trámite y Subetapa; Componentes Reutilizables para
  Excel; Separación de Responsabilidades; Sin IA en el MVP; Mockups como
  Referencia Visual; Alcance Explícito; Ambigüedades Resueltas Antes de
  Implementar; Cumplimiento de la Constitución.
Added sections: Domain and Data Rules; Quality and Scope Rules.
Removed sections: none.
Follow-up TODO: confirm the original ratification date.
-->
# Design Authority Constitution

## Core Principles

### I. Spec-Driven Development
Toda funcionalidad MUST contar con especificación, plan y tareas aprobadas antes
de comenzar su implementación. La aprobación debe quedar registrada como parte
de los artefactos del feature.

### II. Arquitectura en Capas
La aplicación MUST mantener las capas Web MVC, Business/Application,
Repositories y Domain/Entities, con responsabilidades separadas. Esta estructura
mantiene el acoplamiento controlado y hace explícitos los límites de cada capa.

### III. Stack Tecnológico Obligatorio
La solución MUST utilizar C#, .NET 10, ASP.NET Core MVC, SQL Server, Dapper y
Bootstrap/CSS. Cualquier cambio de stack requiere una especificación aprobada y
una enmienda a esta constitución cuando contradiga este principio.

### IV. Sin Entity Framework
La aplicación MUST NOT utilizar Entity Framework para acceso a datos. Dapper es
la tecnología de acceso a datos definida por el stack.

### V. Sin APIs REST Internas por Defecto
La aplicación MUST NOT crear APIs REST para funcionalidad interna, salvo que
una especificación futura aprobada lo requiera explícitamente.

### VI. Repositorios e Inyección de Dependencias
Todo acceso a datos MUST pasar por el patrón Repository y sus dependencias MUST
resolverse mediante Dependency Injection. Ninguna otra capa debe acceder
directamente a la base de datos.

### VII. Protección de Secretos y Validación
Credenciales y cadenas de conexión MUST NOT almacenarse en el código fuente.
Toda entrada MUST validarse en el servidor, independientemente de cualquier
validación realizada en el cliente.

### VIII. Eliminación Lógica y Auditoría
Las entidades funcionales que permitan eliminación MUST implementar soft
delete. Toda modificación de datos MUST registrar el usuario y la fecha
asociados, para conservar trazabilidad.

## Domain and Data Rules

### IX. Scripts de Base de Datos Versionados
Todo script de creación o modificación de la base de datos MUST estar
versionado junto con el proyecto, de modo que los cambios de esquema sean
reproducibles y revisables.

### X. Diseño Integral del Esquema Inicial
El esquema inicial de la base de datos MUST diseñarse integralmente antes de
implementar funcionalidades que dependan de él. La implementación de esas
funcionalidades queda bloqueada hasta que el diseño esté completo y aprobado.

### XI. Usuario Inicial Configurado
En la etapa inicial, la aplicación MUST utilizar un usuario configurado y MUST
NOT implementar un mecanismo de autenticación. La identidad configurada se
utilizará para las necesidades de auditoría.

### XII. Terminología Canónica del Dominio
“Trámite” MUST ser el término canónico del dominio. “Proyecto” y “Proceso”
MUST NOT modelarse como conceptos distintos de Trámite.

### XIII. Relación entre Trámite y Subetapa
Un Trámite MUST poder tener múltiples Subetapas y cada Subetapa MUST pertenecer
a un solo Trámite. El modelo y las restricciones de datos deben preservar esta
relación.

### XIV. Componentes Reutilizables para Excel
Las funciones comunes de Excel MUST implementarse mediante componentes
reutilizables para evitar implementaciones divergentes entre funcionalidades.

### XV. Separación de Responsabilidades
La solución MUST favorecer simplicidad, mantenibilidad, legibilidad y
separación de responsabilidades. Las decisiones de diseño deben evitar
complejidad sin necesidad funcional aprobada.

## Quality and Scope Rules

### XVI. Sin IA en el MVP
El MVP MUST NOT incluir funcionalidades basadas en inteligencia artificial.
Su inclusión requiere una especificación futura aprobada que modifique este
alcance.

### XVII. Mockups como Referencia Visual
Los mockups MUST tratarse como referencia visual y MUST NOT cambiar ni
determinar reglas funcionales. Las reglas funcionales deben proceder de la
especificación aprobada.

### XVIII. Alcance Explícito
La implementación MUST limitarse a las funcionalidades descritas en
especificaciones aprobadas. No se añadirán funcionalidades no descritas ni se
inferirán reglas de negocio para completar vacíos.

### XIX. Ambigüedades Resueltas Antes de Implementar
Toda ambigüedad funcional o de negocio que afecte una decisión de
implementación MUST resolverse en la especificación antes de implementar. La
implementación de la parte afectada queda bloqueada mientras siga sin resolver.

### XX. Cumplimiento de la Constitución
Las especificaciones, planes, tareas y cambios de código MUST verificarse
respecto de esta constitución antes de su aprobación. Un conflicto debe
resolverse mediante aclaración de la especificación o enmienda aprobada de la
constitución; no se permiten excepciones tácitas.

## Governance

Esta constitución gobierna las decisiones de producto y de ingeniería de
Design Authority. Las enmiendas MUST documentar su motivación y su impacto,
recibir aprobación explícita y actualizar la versión y la fecha de última
enmienda. La aprobación de una funcionalidad no sustituye la aprobación de una
enmienda cuando ambas sean necesarias.

La versión utiliza SemVer:
- MAJOR para eliminar o redefinir de forma incompatible un principio vigente.
- MINOR para agregar principios, secciones o requisitos materialmente nuevos.
- PATCH para aclaraciones que no alteren el significado de las reglas.

La revisión de cumplimiento MUST realizarse al aprobar especificación, plan y
tareas, y al revisar la implementación correspondiente. Los conflictos o
ambigüedades deben resolverse antes de avanzar a la implementación afectada.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): fecha original desconocida | **Last Amended**: 2026-10-09
