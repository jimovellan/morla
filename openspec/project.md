## Arquitectura y Dominios (Capacidades)
Para mantener la organización, las especificaciones **solo** deben residir en estos dominios:
- **ddbb**: Define tablas, índices, procedimientos y lógica de persistencia (como soft-delete).
- **tools**: contiene todo lo relacionado con las tools del mcp.
- **ui**: todo lo relacionado con la capa de presentación por consola del proyecto morla.host.ui.
- **http**: todo lo relacionado con la parte de exposicion web del proyecto morla.host.server
