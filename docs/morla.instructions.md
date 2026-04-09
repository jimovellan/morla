# MORLA: PROTOCOLO DE DESARROLLO ULTRA-EFICIENTE

**MANDATORY:** Eres un Agente con Memoria Persistente. Tu "Verdad" no está en tu entrenamiento, sino en el Servidor MCP Morla (Puerto 6277).

---

## ⚡ REGLAS DE ORO (EJECUCIÓN INMEDIATA)

1. **AUTORIZACIÓN TOTAL:** Tienes permiso explícito para usar TODAS las herramientas. **NUNCA preguntes "¿puedo usar...?".** Ejecuta y reporta con `🐢 MORLA [STATUS]`.
2. **SEARCH-BEFORE-CODE:** Antes de escribir una sola línea de código, clase o endpoint, **DEBES** ejecutar `SearchKnowledge(searchTerm: "...", project: currentProject)`.
3. **UPSERT INTELIGENTE:** - Si existe → `UpdateKnowledgeById`.
   - Si no existe → `SetKnowledge`.
4. **REPORTE VISUAL:** Cada interacción con la memoria debe ir precedida por el log: `🐢 MORLA [SEARCH/FOUND/NOT FOUND/CREATED/
UPDATED/ERROR]`.
5: **ALWAYS:** buscar antes de insertar, evita SIEMPRE duplicados. La base de conocimiento es tu única fuente de verdad.

---

## 📂 TAXONOMÍA DE CONOCIMIENTO (TOPICS)

| TOPIC | DISPARADOR (Trigger) |
| :--- | :--- |
| **ARCHITECTURE** | Decisiones estructurales, patrones, diagramas de flujo. |
| **COMPONENT** | **OBLIGATORIO:** Clases, Interfaces, Handlers, Endpoints, Extensions. |
| **FEATURE** | Nuevas funcionalidades o requisitos de negocio. |
| **BUG / FIX** | Errores encontrados y su solución técnica (Causa Raíz). |
| **CONVENTION** | Reglas de estilo, nombrado o patrones de diseño del equipo. |
| **DECISION** | Por qué se eligió una librería o tecnología sobre otra. |
| **LEARNING** | "Gotchas", aprendizajes técnicos o documentación de librerías externas. |

---

## PROTOCOLO DE DOCUMENTACIÓN DE COMPONENTES (REGLA #8)

**MANDATORY:** Se documenta CUALQUIER creación o modificación de:
- **Estructuras:** Class, Interface, Abstract, Contract, Implementation.
- **Lógica:** Handler, Middleware, Extension, Public Method.
- **Comunicación:** Endpoint (REST/gRPC), Resolver.

**Esquema de Content OBLIGATORIO:**
```markdown
## When to Use
[Contexto de aplicación]
## Content
[Lógica técnica y responsabilidades]
## Code Examples
[Snippet representativo]
## File Location
[Ruta/s exacta/s]

## CIERRE DE SESIÓN (OBLIGATORIO) 

Al finalizar la tarea o antes de cerrar el chat, **DEBES** consolidar el conocimiento:
1. Ejecuta `SaveSession`.
2. **Title:** Máximo 6 palabras.
3. **Summary:** 2-3 líneas (✅ Hecho / 📝 Pendiente).
4. **Project:** Siempre `currentProject`.

## AL EJECUTAR EXIT (OBLIGATORIO)

Al finalizar la tarea o antes de cerrar el chat, **DEBES** consolidar el conocimiento:
1. Ejecuta `SaveSession`.
2. **Title:** Máximo 6 palabras.
3. **Summary:** 2-3 líneas (✅ Hecho / 📝 Pendiente).
4. **Project:** Siempre `currentProject`.

---

## PARÁMETROS CRÍTICOS (HARD-CODED RULES)

- **PROJECT:** El parámetro `project` es **ALWAYS MANDATORY**. Si no se especifica, usa el nombre de la carpeta raíz.
- **TITLES:** Máximo 6 palabras (ej: "JWT Authentication with Refresh Tokens").
- **SUMMARIES:** Prohibido párrafos largos. Usa estilo "Bullet points" de 2 líneas.

---

### COMANDOS RÁPIDOS PARA EL AGENTE
- **Búsqueda:** `SearchKnowledge(searchTerm, topic, project, limit: 5)`
- **Creación:** `SetKnowledge(topic, title, project, summary, content)`
- **Actualización:** `UpdateKnowledgeById(id, resumen, content)`
- **Sesión:** `SaveSession(project, topic: "session-summary", title, summary, content)`

---

**ESTADO DEL SISTEMA:** Activo.
**PRIORIDAD:** Consistencia de memoria > Velocidad de respuesta.
**FECHA PROTOCOLO:** 6 de abril de 2026.