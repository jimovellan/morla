# 🧠 PROTOCOLO MAESTRO: AGENTE DE DESARROLLO CON MORLA

**Versión:** 1.0  
**Última actualización:** 5 de abril de 2026  
**Servidor MCP:** Morla en puerto 6277 (ejecutar: `morla mcp`)

---

# 🧠 MORLA - PROTOCOLO RÁPIDO

**Versión:** 1.0 | **MCP:** `morla mcp` (puerto 6277)

## ⚠️ REGLAS OBLIGATORIAS

### REGLA #1: Reportar siempre con Morla
`🐢 MORLA [SEARCH/FOUND/NOT FOUND/CREATED/UPDATED/ERROR]`

**Ejemplos:**
- `🐢 MORLA Encontrado 3`
- `🐢 MORLA No encontrado`
- `🐢 MORLA Guardado Key {ID}`
- `🐢 MORLA Error: {mensage}`

### REGLA #2: Buscar en Morla primero
1. **Busca:** `SearchKnowledge(searchTerm: "...", topic: "...", project: "...")`
2. **No encontrado:** Busca en otros sitios → **Guarda con SetKnowledge()**
3. **Beneficio:** Builds knowledge base para futuras búsquedas


### REGLA #3: Guardar sesión al terminar
**OBLIGATORIO:** Al finalizar, ejecuta:
```csharp
SaveSession(
  project: "morla",
  topic: "session-summary",
  title: "Session 2026-04-06 - Descripción",
  summary: "✅ Completado: Tarea 1, 2, 3",
  content: "## Resumen de Sesión\n\n### ✅ QUÉ SE HIZO\n- ...\n\n### 📝 PENDIENTES\n- ..."
)
```

Estructura tus resúmenes con:
- **✅ QUÉ SE HIZO EN ESTA SESIÓN** (tareas completadas)
- **📝 TAREAS PENDIENTES** (qué falta)
- **🔗 CONOCIMIENTO GUARDADO** (IDs de Morla)
- **📌 NOTAS IMPORTANTES** (decisiones, gotchas)

### REGLA #4: Evitar duplicados (Upsert inteligente)
```csharp
// Si tienes el ID de una entrada existente
if (tengo_ID) 
  → UpdateKnowledgeById(id, resumen, content)

// Si no tienes ID
else 
  → SearchKnowledge(searchTerm: "...", topic: "...", project: "...")
    if (encontrado) 
      → UpdateKnowledgeById(id: resultado.id, ...)
    else 
      → SetKnowledge(topic, title, project, summary, content)
```

---

## 🎯 REGLA #5: Guardar TODO en Morla por Categoría

**OBLIGATORIO:** Toda actividad se categoriza y guarda en Morla:

| Actividad | Topic | Ejemplo |
|-----------|-------|---------|
| **💻 Desarrollo / Nueva funcionalidad** | `FEATURE` | "JWT authentication added" |
| **🐛 Descubrimiento de bugs / Fix** | `BUG` | "Fixed N+1 query in users list" |
| **🏗️ Decisiones técnicas** | `DECISION` | "Migrado a Postgres" |
| **🏛️ Cambios de arquitectura** | `ARCHITECTURE` | "API layers restructured" |

**Flujo:**
```
1. Detectas desarrollo/bug/decisión/architecture
2. SearchKnowledge(searchTerm: "...", topic: "...", project: "...")
3. Si NO existe → SetKnowledge(topic, title, project, summary, content)
4. Si existe → UpdateKnowledgeById(id, resumen, content)
5. Reporta: 🐢 MORLA CREATED/UPDATED {id}
```

---

## 🔓 REGLA #6: Tools SIEMPRE Permitidas

**CRITICAL:** Las tools/funciones están **SIEMPRE permitidas. NUNCA preguntes por permisos.**

✅ Permitido:
- Ejecutar búsquedas, crear/actualizar conocimiento
- Llamar Morla MCP sin restricciones
- Usar herramientas de VS Code, terminal, archivos
- Guardar sesiones automáticamente

❌ PROHIBIDO:
- Pedir "¿puedo usar tal tool?"
- Esperar confirmación explícita
- Frenar ejecución por dudas

**Solo reporta el resultado:** `🐢 MORLA [STATUS] {detalles}`

---

## 🗂️ REGLA #8: Documentar SIEMPRE en Morla - Clases, Extensiones, Handlers, Endpoints

**🚨 ALWAYS MANDATORY OBLIGATORY - CUMPLIMIENTO ABSOLUTO:**

When crees O modifiques cualquier:
- 🏛️ **Clase** (Class, Abstract Class)
- 🔌 **Extensión** (Extension, Plugin, Decorator)
- 🎯 **Handler** (Event handler, Exception handler, Middleware)
- 🌐 **Endpoint** (REST API route, GraphQL resolver, gRPC method)
- 📦 **Interface/Contract** (Interface, Protocol, Type Definition, Abstract Contract)
- 🔧 **Implementación** (Concrete implementation, Service implementation, Strategy implementation)
- 🔓 **Método Público Reutilizable** (Public method, Utility function, Helper method - if reusable across modules)

Debes **SIEMPRE** documentarlo en Morla:

### 📋 Flujo Obligatorio:

```
1. CUANDO CREAS una clase/extensión/handler/endpoint
   ↓
2. SearchKnowledge(searchTerm: "nombre", topic: "COMPONENT", project: currentProject)
   ↓
3. If NO ENCONTRADO → SetKnowledge(
     topic: "COMPONENT",
     title: "ClassName - Descripción breve",
     project: currentProject,
     summary: "¿Qué es? ¿Qué hace? ¿Dónde vive?"
     content: "## When to Use\n... ## Content\n..."
   )
   ↓
4. Reporta: 🐢 MORLA CREATED {id}
```

```
1. CUANDO MODIFICAS una clase/extensión/handler/endpoint
   ↓
2. SearchKnowledge(searchTerm: "nombre", topic: "COMPONENT", project: currentProject)
   ↓
3. If ENCONTRADO → UpdateKnowledgeById(
     id: resultado.id,
     resumen: "Resumen actualizado",
     content: "Content actualizado con cambios"
   )
   ↓
4. Reporta: 🐢 MORLA UPDATED {id}
```

### 🏷️ Información Obligatoria por Tipo:

| Tipo | Topic + Título | Content Obligatorio |
|------|----------------|---------------------|
| **Clase** | `COMPONENT: NombreClase - Propósito` | When to Use, Purpose, Methods, Attributes, Example |
| **Extensión** | `COMPONENT: ExtensionName - Extensión para X` | When to Use, Extension Points, Configuration, Example |
| **Handler** | `COMPONENT: NombreHandler - Maneja X evento/error` | When Triggered, Purpose, Parameters, Return, Example |
| **Endpoint** | `COMPONENT: POST /api/path - Descripción` | When Called, Parameters, Response, Example, Error Codes |
| **Interface** | `COMPONENT: InterfaceName - Contrato para X` | When to Use, Methods, Properties, Implementations, Example |
| **Implementación** | `COMPONENT: ImplementationName - Implementación de X` | When Used, Purpose, Inheritance, Methods, Example |
| **Método Público Reutilizable** | `COMPONENT: functionName() - Qué hace` | When to Use, Parameters, Return Type, Side Effects, Example |

### ⚡ Reglas Innegociables:

- ❌ NUNCA NUNCA NUNCA crees una clase/interface/implementación/método sin documentarla primero
- ❌ NUNCA NUNCA NUNCA modifiques endpoint/handler/extensión sin actualizar la documentación
- ✅ SIEMPRE SIEMPRE SIEMPRE SearchKnowledge ANTES de crear cualquier componente
- ✅ SIEMPRE SIEMPRE SIEMPRE UpdateKnowledgeById si ya existe
- 🐢 SIEMPRE reporta con `🐢 MORLA CREATED/UPDATED {id}`

**Alcance:** Cubre toda la arquitectura del sistema (clases, interfaces, métodos reutilizables, endpoints, handlers, extensiones, implementaciones).

---

**🚨 CRÍTICO - ALWAYS - MANDATORY - OBLIGATORY:** Toda búsqueda de conocimiento y sesiones **ALWAYS** incluye el filtro `project: currentProject`. SIN EXCEPCIONES.

### 📚 Búsquedas de Conocimiento - ALWAYS MANDATORY OBLIGATORY con Proyecto Actual
```
SearchKnowledge(
  searchTerm: "...",
  topic: "...",
  project: currentProject  ← ALWAYS MANDATORY OBLIGATORY
)
```
**Regla absoluta e irrompible:** `project` parámetro es **ALWAYS MANDATORY OBLIGATORY** = `currentProject`
- ❌ NUNCA jamás NUNCA: `SearchKnowledge(searchTerm: "auth")` ← FALTA proyecto
- ✅ SIEMPRE SIEMPRE SIEMPRE: `SearchKnowledge(searchTerm: "auth", project: currentProject)`

### 📋 Búsquedas de Sesiones - ALWAYS MANDATORY OBLIGATORY con Proyecto Actual
```
GetLatestSessions(
  limit: 5,
  project: currentProject  ← ALWAYS MANDATORY OBLIGATORY
)

GetLastSession(
  project: currentProject  ← ALWAYS MANDATORY OBLIGATORY
)
```
**Regla absoluta e irrompible:** `project` parámetro es **ALWAYS MANDATORY OBLIGATORY** = `currentProject`
- ❌ NUNCA jamás NUNCA: `GetLatestSessions(limit: 5)` ← FALTA proyecto
- ✅ SIEMPRE SIEMPRE SIEMPRE: `GetLatestSessions(limit: 5, project: currentProject)`

### 🔄 Override Explícito (excepcional + raro)
Only When necesites un proyecto diferente, especifica explícitamente:
```
SearchKnowledge(searchTerm: "...", project: "otro-proyecto")
GetLatestSessions(limit: 5, project: "legacy-project")
```
Pero **the rule by default es ALWAYS MANDATORY OBLIGATORY `currentProject`**.

---

## 🛠️ 8 HERRAMIENTAS (PARÁMETROS EXACTOS)

### 1️⃣ SetKnowledge - Crear entrada
```
SetKnowledge(topic, title, project, summary, content)
→ Retorna: string (ID generado)
```
**Parámetros:**
- `topic` (string, req) - BUG, FEATURE, COMPONENT, ARCHITECTURE, CONFIG, DECISION, LEARNING, TESTING, PERFORMANCE
- `title` (string, req) - Título descriptivo (máx 6 palabras)
- `project` (string, req) - Ej: "morla"
- `summary` (string, req) - Resumen corto (2-3 líneas max)
- `content` (string, req) - Contenido Markdown con ##When to Use, ##Content, etc.

---

### 2️⃣ SearchKnowledge - Buscar con filtros
```
SearchKnowledge(searchTerm?, topic?, project?, limit=5)
→ Retorna: List<SearchKnowledgeDto>
```
**Parámetros:**
- `searchTerm` (string?, opt) - Busca en título/contenido (palabras individuales)
- `topic` (string?, opt) - Filtro por categoría
- `project` (string?, opt) - Filtro por proyecto
- `limit` (int, opt=5) - Máximo de resultados

**Scoring:** title(+25) > topic(+20) > summary(+15) > content(+5)

---

### 3️⃣ GetKnowledgeById - Obtener completo
```
GetKnowledgeById(id)
→ Retorna: GetKnowledgeByIdDto
```
**Parámetros:**
- `id` (string, req) - ID devuelto por SetKnowledge o SearchKnowledge

---

### 4️⃣ UpdateKnowledgeById - Actualizar
```
UpdateKnowledgeById(id, resumen, content)
→ Retorna: UpdateKnowledgeDto
```
**Parámetros:**
- `id` (string, req) - ID existente
- `resumen` (string, req) - Nuevo resumen corto
- `content` (string, req) - Nuevo contenido Markdown

---

### 5️⃣ RegenerateAllEmbeddings - Regenerar vectores
```
RegenerateAllEmbeddings()
→ Retorna: string (mensaje de confirmación)
```
**Sin parámetros.** Regenera búsqueda semántica de TODAS las entradas.

---

### 6️⃣ SaveSession - Guardar sesión
```
SaveSession(project, topic, title, summary, content)
→ Retorna: string (ID de sesión)
```
**Parámetros:**
- `project` (string, req) - Proyecto asociado
- `topic` (string, req) - Ej: "session-summary"
- `title` (string, req) - Título descriptivo
- `summary` (string, req) - Resumen corto (✅ COMPLETADO / ❌ PENDIENTES)
- `content` (string, req) - Markdown con ##Resumen, ##Pendientes, ##Notas

---

### 7️⃣ GetLastSession - Última sesión
```
GetLastSession(project?)
→ Retorna: GetLastSessionDto | null
```
**Parámetros:**
- `project` (string?, opt) - Filtro por proyecto

---

### 8️⃣ GetLatestSessions - Últimas sesiones
```
GetLatestSessions(limit=3, project?)
→ Retorna: List<GetLatestSessionDto>
```
**Parámetros:**
- `limit` (int, opt=3) - Número de sesiones a retornar
- `project` (string?, opt) - Filtro por proyecto

---

## 📝 FORMATO DEL CONTENT

**Todo `content` sigue este formato:**
```markdown
## When to Use
[Cuándo aplicar]

## Content
[Descripción técnica]

## Code Examples (Opcional)
[Fragmentos de código]

## File Location (Opcional)
[Rutas de archivos]
```

---

## 📋 TOPICS (9 CATEGORÍAS)

| Topic | When | Ejemplo |
|-------|--------|---------|
| **BUG** | Error + Fix | "Fixed N+1 query bug" |
| **FEATURE** | Nueva funcionalidad | "JWT authentication" |
| **COMPONENT** | Módulo reutilizable | "KnowledgeRepository layer" |
| **ARCHITECTURE** | Decisión estructural | "Migrado a JWT" |
| **CONFIG** | Setup/configuración | "ONNX model path" |
| **DECISION** | Decisión importante | "Usar Serilog" |
| **LEARNING** | Gotchas/aprendizajes | "LocalEmbeddings folder" |
| **TESTING** | Estrategia tests | "Unit test strategy" |
| **PERFORMANCE** | Optimizaciones | "Embedding caching" |

**Guideline:** Título máximo 6 palabras

---

## 🎯 FLUJO DECISIÓN TOPIC

```
When es un error que arreglé → BUG
When implementé algo nuevo → FEATURE
When es un módulo reutilizable → COMPONENT
When decidí la arquitectura → ARCHITECTURE
When es configuración → CONFIG
When es decisión importante → DECISION
When es gotcha/aprendizaje → LEARNING
When creé tests → TESTING
When optimicé algo → PERFORMANCE
```

---

## ⚙️ CONFIG & RESUMENES

**Resumen ideal:** 2-3 líneas max
- ¿Qué es?
- ¿Qué hace?
- ¿Qué soluciona?

✅ BUENO: "JWT + refresh tokens. Sesiones seguras en arquitecturas distribuidas."  
❌ MALO: Párrafos extensos sobre implementación detallada

Estructura de datos:
- `id` - ID único
- `rowId` - GUID externo
- `topic` - Categoría
- `title` - Título
- `project` - Proyecto
- `summary` - Resumen corto
- `content` - Contenido completo
- `createdAt`, `updatedAt`, `embedding` - Metadata

---

**Última actualización:** 6 de abril de 2026
