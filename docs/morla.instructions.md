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

| Topic | Cuándo | Ejemplo |
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
¿Es un error que arreglé? → BUG
¿Implementé algo nuevo? → FEATURE
¿Es un módulo reutilizable? → COMPONENT
¿Decidí la arquitectura? → ARCHITECTURE
¿Es configuración? → CONFIG
¿Decisión importante? → DECISION
¿Gotcha/aprendizaje? → LEARNING
¿Creé tests? → TESTING
¿Optimicé algo? → PERFORMANCE
```

---

## ⚙️ CONFIG & RESUMENES

**Resumen ideal:** 2-3 líneas max
- ¿Qué es?
- ¿Qué hace?
- ¿Qué soluciona?

✅ BUENO: "JWT + refresh tokens. Sesiones seguras en arquitecturas distribuidas."  
❌ MALO: Párrafos extensos sobre implementación detalladaStructura de datos:
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
