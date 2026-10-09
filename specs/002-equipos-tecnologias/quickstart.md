# Quickstart de validación: Equipos y Tecnologías

## Estado

Este documento describe las verificaciones que deberán ejecutarse al
implementar la feature. El repositorio todavía no contiene `.sln`, proyectos
.NET, scripts de base de datos ni un framework de pruebas, por lo que los
comandos ejecutables se establecerán cuando se creen esos artefactos. No
ejecutar `dotnet test` hasta que exista una solución aprobada.

## Prerrequisitos para la validación futura

- Especificación, plan y tareas aprobados.
- Esquema inicial integral aprobado e integrado, incluidos TeamTechnology,
  Role, Person y usuario configurado.
- SQL Server de pruebas y configuración de conexión externa al código fuente.
- Proyectos Web, Application, Domain e Infrastructure y herramienta de pruebas
  seleccionada.

## Escenarios de comprobación

| # | Escenario | Resultado esperado |
|---:|---|---|
| 1 | Leer catálogos iniciales | Existen ocho opciones y quince Roles asociados como en [data-model.md](./data-model.md). |
| 2 | Crear persona válida por cada Equipo/Tecnología | Cada alta acepta exactamente una opción y un Role asociado; la fecha de alta se asigna y el Id queda único. |
| 3 | Omitir apellido materno | Alta y presentación continúan válidas; el resto de campos es visible. |
| 4 | Omitir campo obligatorio o elegir un Role de otra opción | Se informa validación y no se crea ni modifica parcialmente la persona. |
| 5 | Modificar persona | Cambios funcionales permitidos se guardan; Id y fecha de registro no cambian. |
| 6 | Asignar dos personas activas al mismo Role de liderazgo | La segunda operación se rechaza, incluso al ejecutar altas concurrentes. |
| 7 | Asignar varias personas a un Role múltiple | Desarrollador, Arquitecto, Release manager y Tester admiten múltiples activos. |
| 8 | Eliminar una persona activa | Queda inactiva, se registran fecha/usuario de baja, deja de aparecer en el directorio activo y libera capacidad de liderazgo. |
| 9 | Auditar alta, cambio y baja | Se asocia el usuario configurado y se conservan los campos de auditoría aplicables, sin autenticación. |

## Capas que valida el recorrido

1. MVC presenta el directorio y formularios, valida model state y presenta
   errores sin exponer acceso a SQL.
2. Application coordina alta, consulta, modificación y baja lógica, obtiene
   identidad configurada y aplica validaciones dependientes del catálogo.
3. Domain representa Persona, Equipo o Tecnología, Rol y reglas de
   cardinalidad confirmadas.
4. Infrastructure ejecuta consultas parametrizadas Dapper mediante
   repositorios; usa transacciones y bloqueo por Role para asignación única de
   liderazgo.

## Gate de ejecución

No hay pruebas ejecutables todavía. Tras completar solución, esquema integral
y tareas, agregar comandos concretos de compilación/pruebas y ejecutar la
matriz anterior. La aprobación de este quickstart no reemplaza la aprobación
de la especificación, el plan ni las tareas.
