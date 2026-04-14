PROTOCOLO MORLA: GESTIÓN OBLIGATORIA DE CONOCIMIENTO ANTES DE CUALQUIER HERRAMIENTA

Versión: 3.0
Fecha: 14 de abril de 2026
Servidor MCP: Morla puerto 6277 (ejecutar: dotnet run mcp)

REGLA DE PRIORIDAD ABSOLUTA: Busca SIEMPRE en Morla primero. Nunca uses herramientas externas sin agotar la búsqueda en Morla primero. Sin excepciones.

---POLÍTICA DE LENGUAJE---

TODO debe guardarse en CASTELLANO excepto:
  - Nombres de variables (nombreVariable, myFunction, etc)
  - Conceptos de sistema (JWT, API, REST, JSON, etc)
  - Claves de configuración (apiKey, database_url, etc)
  - Nombres de librerías y frameworks (React, MongoDB, .NET, etc)
  - IDs de Morla y referencias técnicas internas

Ejemplo correcto:
  Título: "Autenticación JWT con tokens de actualización"
  Contenido: "Implementa autenticación segura usando biblioteca JWT..."
  Variable: nombreUsuario, tokenExpiracion, apiToken

---FLUJO DE BÚSQUEDA OBLIGATORIO (ANTES DE CUALQUIER HERRAMIENTA)---

Este flujo es imprescindible. Ejecuta SIEMPRE para toda solicitud que requiera información o generación de conocimiento.

PASO 1: SIEMPRE Búsqueda Multipassada (Obligatorio 3-5 pasadas)
Ejecuta SIEMPRE entre 3 y 5 iteraciones de búsqueda con variaciones de términos sinónimos.
NUNCA uses el parámetro topic en las búsquedas. El filtro de proyecto es SIEMPRE obligatorio.

Ejemplo de iteraciones para "error en autenticación":
  Pasada 1: SearchKnowledge(searchTerm="autenticación", project="NombreProyectoActual")
  Pasada 2: SearchKnowledge(searchTerm="auth", project="NombreProyectoActual")
  Pasada 3: SearchKnowledge(searchTerm="login sesión", project="NombreProyectoActual")
  Pasada 4: SearchKnowledge(searchTerm="gestión de sesiones", project="NombreProyectoActual")
  Pasada 5: SearchKnowledge(searchTerm="JWT token", project="NombreProyectoActual")

Razón: Cada pasada con términos distintos maximiza el recall semántico del RAG y detecta variaciones en títulos y resúmenes almacenados.

PASO 2: Evaluar Resultados
Compila todos los resultados de las 3-5 pasadas. Si encuentra coincidencia:
  → GetKnowledgeById(id_coincidencia)
  → Revisa contenido completo
  → Determina: ¿Actualizar existente O crear nuevo si es genuinamente diferente?

PASO 3: Decisión Upsert
Si encuentras ID de conocimiento existente:
  → UpdateKnowledgeById(id, nuevo_resumen, nuevo_contenido)
Si no hay coincidencia después de TODAS las pasadas:
  → SetKnowledge(topic, título, project="NombreProyectoActual", resumen, contenido)

PASO 4: Después de usar herramientas externas
Si obtuviste NUEVA información usando herramientas (grep, file_search, bash, etc):
  → Vuelve al PASO 1 para verificar que no exista ya esa información
  → Si no existe: guarda en Morla con SetKnowledge()
  → Documenta fuente, contexto y rutas de archivos relevantes

PASO 5: Cierre de sesión (OBLIGATORIO al finalizar)
Al terminar la conversación o una tarea significativa:
  → SaveSession(project="NombreProyectoActual", topic, title, summary, content)
  → Registra: qué se hizo, decisiones tomadas, pendientes si los hay

---OPERACIONES DE ALMACENAMIENTO OBLIGATORIO---

Guarda inmediatamente en Morla cuando:
  - Obtengas información nueva (investigación, resultados de búsqueda)
  - Generes conocimiento nuevo (soluciones, patrones, enfoques)
  - Tomes decisiones (arquitectónicas, de implementación, de proceso)
  - Resuelvas errores (causa raíz + solución)
  - Descubras limitaciones o gotchas
  - Implementes características (funcionalidad + cómo funciona)
  - Optimices rendimiento (métricas + enfoque)
  - Revises código (hallazgos, patrones, problemas detectados)
  - Configures herramientas (pasos de instalación, gotchas)
  - Definas estrategias de pruebas
  - Establezcas patrones arquitectónicos

Formato de almacenamiento: SetKnowledge() para nuevo, UpdateKnowledgeById() para existente.
El proyecto SIEMPRE debe ser el proyecto activo actual, no un placeholder.

---CLASIFICACIÓN DE TÓPICOS (Solo para almacenamiento, NUNCA para búsqueda)---

Los tópicos son EXCLUSIVAMENTE para clasificación al guardar. NUNCA se pasan como parámetro en SearchKnowledge.

Tópicos válidos:
  bug          - Error + causa raíz + solución
  feature      - Funcionalidad nueva + mecánica
  component    - Módulo reutilizable + interfaces
  architecture - Decisión estructural + compensaciones
  config       - Instalación + detalles de configuración
  decision     - Decisión importante + contexto
  learning     - Gotcha o limitación descubierta
  testing      - Estrategia de pruebas + enfoque
  performance  - Optimización + mediciones
  pattern      - Patrón reutilizable o enfoque
  integration  - Integración de terceros + instalación
  convention   - Regla de estilo, naming o proceso
  requirement  - Requisito de negocio o funcional
  workflow     - Flujo de trabajo o proceso definido
  discovery    - Descubrimiento exploratorio del código base

---FORMATO DE CONTENIDO (Markdown Obligatorio)---

Título: Máximo 5-6 palabras. Explícito y buscable. En castellano.

Resumen: Máximo 2-3 líneas.
  Estructura: ¿Qué es? ¿Qué hace? ¿Qué problema resuelve?
  Ejemplo correcto: "Autenticación JWT. Proporciona seguridad de sesión sin estado en arquitecturas de microservicios."
  Ejemplo incorrecto: "Esta es la autenticación con tokens que hicimos para el proyecto."

Contenido (Markdown estructurado obligatorio):
  ## Cuándo Usar
  [Escenarios, disparadores, casos de aplicación]
  
  ## Detalles Técnicos
  [Descripción técnica completa]
  
  ## Ejemplos de Código (Opcional)
  [Fragmentos de código o referencias de archivos]
  
  ## Ubicaciones de Archivos (Opcional)
  [Rutas relativas a archivos relevantes del proyecto]
  
  ## Compensaciones (Si aplica)
  [Ventajas, desventajas, consideraciones]

---HERRAMIENTAS DISPONIBLES---

SearchKnowledge(searchTerm, project)
  - NUNCA usar parámetro topic en búsquedas
  - El parámetro project es SIEMPRE obligatorio
  - Ejecuta 3-5 pasadas con variaciones de términos sinónimos
  - Ejecutar en paralelo cuando sea posible para mayor eficiencia

GetKnowledgeById(id)
  - Obtiene contenido completo después de encontrar coincidencia en búsqueda
  - Úsalo siempre antes de decidir si actualizar o crear nuevo

SetKnowledge(topic, title, project, summary, content)
  - Crea conocimiento nuevo
  - Requiere todos los campos: topic, title, project, summary, content
  - Solo usar si la búsqueda multipassada no encontró nada relevante

UpdateKnowledgeById(id, resumen, content)
  - Actualiza conocimiento existente
  - El content reemplaza COMPLETAMENTE el anterior; incluye todo lo que quieras conservar
  - Usar siempre que exista ya una entrada relacionada

SaveSession(project, topic, title, summary, content)
  - Persiste el contexto de la sesión actual para recuperación futura
  - Llamar obligatoriamente al finalizar tareas significativas
  - Registra qué se hizo, decisiones, pendientes

GetLastSession(project)
  - Recupera el contexto de la última sesión guardada
  - Llamar al inicio si el usuario retoma trabajo previo

RegenerateAllEmbeddings()
  - Reconstruye el índice semántico completo
  - Usar SOLO si hay problemas con la búsqueda semántica o tras actualizaciones masivas
  - Operación pesada; no usar en flujo normal

---COMPUERTAS DE CALIDAD---

Antes de guardar conocimiento verificar:
  ✓ El resumen tiene máximo 2-3 líneas
  ✓ El título tiene 5-6 palabras o menos y es en castellano
  ✓ El topic es uno de los tópicos válidos listados arriba
  ✓ El project está explícitamente asignado al proyecto actual (no un placeholder)
  ✓ El content está estructurado con encabezados markdown
  ✓ No hay duplicados: se realizaron 3-5 búsquedas exhaustivas antes

Antes de usar herramientas externas (grep, bash, file_search, etc) verificar:
  ✓ Se completó la búsqueda multipassada en Morla (mínimo 3 pasadas)
  ✓ No se encontró conocimiento relevante en ninguna pasada
  ✓ Los términos de búsqueda utilizados fueron variados y sinónimos

---LISTA DE VERIFICACIÓN DE EJECUCIÓN---

Para cada solicitud del usuario:
  1. INICIO: Ejecuta búsqueda multipassada en Morla (3-5 pasadas, sin topic, con project)
  2. Si encuentras: GetKnowledgeById() → revisa → decide actualizar o crear nuevo
  3. Si no encuentras: usa herramientas externas según sea necesario
  4. Después de herramientas: guarda hallazgos nuevos en Morla (verificando duplicados)
  5. CIERRE: SaveSession() al finalizar tarea significativa

Esto es obligatorio. Sin excepciones a menos que el usuario lo deshabilite explícitamente.

---NOTAS IMPORTANTES---

Búsquedas sin tópicos: Las búsquedas en Morla NUNCA usan tópicos. Los tópicos son solo para organizar en almacenamiento.
Variaciones de términos: Diferentes búsquedas encuentran diferentes resultados. Ser exhaustivo es obligatorio.
Proyecto obligatorio: Cada operación debe especificar el proyecto activo para evitar contaminación entre proyectos.
Lenguaje consistente: Documentación en castellano, nombres técnicos en inglés.
Paralelismo: Cuando las búsquedas son independientes entre sí, lanzarlas en paralelo para mayor eficiencia.