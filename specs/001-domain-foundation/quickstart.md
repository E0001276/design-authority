# Quickstart: Validación de Fundamentos de Dominio

## Estado

Guía de validación prevista. El repositorio aún no tiene solución .NET,
proyectos, scripts SQL ni pruebas; los comandos se podrán ejecutar después de
que `/speckit-tasks` y la implementación creen esos artefactos. Esta guía no
crea código ni modifica la base de datos.

## Prerrequisitos para la futura implementación

- .NET 10 SDK.
- SQL Server de desarrollo/pruebas accesible.
- Cadena de conexión configurada fuera del código fuente.
- Scripts de base de datos versionados disponibles después de aprobar el
  esquema integral.

## Validaciones previstas

1. **Capas y referencias**: compilar la solución y verificar que Domain no
   depende de Infrastructure ni Web; Application declara los contratos usados
   por casos de uso.
2. **Trámite/Subetapa**: probar la cardinalidad Subetapa→Trámite y que el
   selector de Subetapa para Hallazgo solo contemple Subetapas activas del
   mismo Trámite.
3. **Trámite predeterminado**: probar máximo uno activo predeterminado,
   posibilidad de cero después de desactivarlo, selección inicial si existe y
   preservación de las asociaciones históricas tras un cambio. No probar nombre
   fijo ni flujo de creación cuando falte predeterminado, pues no están
   definidos.
4. **Tecnologías**: verificar las cuatro entradas iniciales y asociaciones
   especificadas, sin relaciones adicionales.
5. **Auditoría y seguimiento**: verificar que seguimientos de Hallazgos y
   Acuerdos se persisten independientemente y conservan fecha, usuario y
   observación; validar auditoría y soft delete solo para entidades que la
   especificación defina como eliminables.
6. **Minuta**: reemplazar el archivo vigente de prueba y comprobar que solo se
   conserva el archivo más reciente y sus metadatos requeridos, con referencia
   relativa. Confirmar que no se conserva el archivo anterior.
7. **Exclusiones**: revisar que no haya CRUD funcional, autenticación, REST,
   IA, funcionalidad de Excel ni pantallas de Informe Semanal.

## Comandos previstos después de crear la solución

Los nombres de proyectos y scripts se fijarán en las tareas de implementación.
Como guía, desde la raíz del repositorio:

```powershell
dotnet build
dotnet test
```

No ejecutar scripts contra una base real hasta que el modelo integral, sus
campos y las relaciones pendientes hayan sido aprobados y el ambiente esté
configurado.

## Resultado esperado

Las pruebas de dominio y persistencia confirman las reglas definidas en
[data-model.md](./data-model.md). Toda regla marcada como pendiente permanece
fuera del comportamiento implementado hasta que se apruebe en la feature
correspondiente.
