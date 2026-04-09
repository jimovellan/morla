# 📚 MORLA MCP - MANUAL DE INICIO

Bienvenido al MCP de Morla. Este servidor expone herramientas de gestión de conocimiento para tu agente IA.

## 🎯 TOOLS DISPONIBLES

### 1. **SetKnowledge** - Crear/guardar conocimiento
Guardar nueva regla, decisión, bug fix, funcionalidad o aprendizaje.

**Parámetros:**
- `topic`: architecture | bug | fix | convention | workflow | discovery | requirement
- `title`: Máximo 5 palabras
- `project`: Nombre del proyecto actual (OBLIGATORIO)
- `summary`: Máximo 2 líneas
- `content`: Markdown con ##When to Use, ##Content, ##Code Examples

**Ejemplo:**
```
SetKnowledge(
  topic: "architecture", 
  title: "JWT autenticación con refresh",
  project: "morla",
  summary: "JWT + refresh tokens para seguridad sin estado",
  content: "## When to Use\n..."
)
```

---

### 2. **SearchKnowledge** - Buscar en la base de conocimiento
SIEMPRE busca antes de crear/actualizar (evita duplicados).

**Parámetros:**
- `searchTerm`: Palabras clave (opcional)
- `topic`: Filtro por categoría (opcional)
- `project`: Tu proyecto actual (CRÍTICO - incluir siempre)
- `limit`: Máximo 5 resultados (default)

**Flujo recomendado:**
1. `SearchKnowledge(searchTerm: "concepto", project: currentProject)`
2. Si ENCONTRADO → `GetKnowledgeById(id)` → `UpdateKnowledgeById()` si necesita cambios
3. Si NO ENCONTRADO → `SetKnowledge()` para crear

---

### 3. **GetKnowledgeById** - Ver entrada completa
Obtiene todos los detalles después de una búsqueda.

**Parámetros:**
- `id`: ID devuelto por SearchKnowledge

---

### 4. **UpdateKnowledgeById** - Actualizar entrada existente
Modifica resumen y contenido de entrada.

**⚠️ IMPORTANTE:** El content se reemplaza completamente - incluye TODO lo que quieras conservar.

**Parámetros:**
- `id`: ID de entrada existente
- `resumen`: Nuevo resumen (2-3 líneas)
- `content`: Contenido COMPLETO en Markdown

---

### 5. **RegenerateAllEmbeddings** - Regenerar índice semántico
SOLO para mantenimiento. NO llamar normalmente (se actualiza automáticamente).

---

## 📋 TOPICS VÁLIDOS

| Topic | Cuándo | Ejemplo |
|-------|--------|---------|
| **architecture** | Decisiones estructurales | "JWT auth flow" |
| **bug** | Errores encontrados | "N+1 query encontrado" |
| **fix** | Solución a bugs | "Fixed: Added index on email" |
| **convention** | Reglas de código/estilo | "Logging: Class.Method pattern" |
| **workflow** | Procesos repetidos | "Release: tag → push → publish" |
| **discovery** | Gotchas/aprendizajes | "Nullable refs gotcha" |
| **requirement** | Requisitos negocio | "Max file: 250MB" |

---

## 🔄 FLUJOS TÍPICOS

### FLUJO 1: Documentar una decisión arquitectónica
```
1. SearchKnowledge(searchTerm: "JWT", project: "morla")
   ↓ Si NO existe:
2. SetKnowledge(topic: "architecture", title: "JWT auth", project: "morla", summary: "...", content: "...")
   ↓ Copilot recibe ID
3. Usa el ID para futuros updates
```

### FLUJO 2: Actualizar cuando cambian las reglas
```
1. SearchKnowledge(searchTerm: "logging pattern", topic: "convention", project: "morla")
2. GetKnowledgeById(id)  ← Lee contenido anterior
3. UpdateKnowledgeById(id, resumen: "ACTUALIZADO: ...", content: "... (todo lo anterior) ... (cambios nuevos)")
```

### FLUJO 3: Consultar reglas antes de implementar
```
1. SearchKnowledge(searchTerm: "error handling", topic: "convention", project: "morla")
2. GetKnowledgeById(id)
3. Lee las reglas y implementa siguiendo esas pautas
```

---

## ✅ CHECKLIST AL CONECTAR

- ✅ Leo SIEMPRE antes de crear (SearchKnowledge)
- ✅ Categorizo con topics correctos (architecture, bug, fix, etc.)
- ✅ Incluyo project SIEMPRE en búsquedas
- ✅ Resúmenes máximo 2-3 líneas
- ✅ Content en Markdown con secciones claras
- ✅ Al actualizar, copio content anterior + nuevos cambios
- ✅ Guardo URLs, líneas de código, referencias en content

---

## 🚀 PRÓXIMOS PASOS

1. Busca conocimiento existente: `SearchKnowledge(...)`
2. Si no existe, crea: `SetKnowledge(...)`
3. Si existe, actualiza si es necesario: `UpdateKnowledgeById(...)`
4. Consulta antes de implementar cambios importantes

**¡Bienvenido a Morla! 🐢**
