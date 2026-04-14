/**
 * Morla — OpenCode plugin adapter
 *
 * Injects Morla's agent rules from ~/.config/opencode/memory.instructions.md
 * into every system prompt, ensuring the agent ALWAYS follows Morla's protocol.
 *
 * Flow:
 *   System prompt transform → read Morla instructions → inject into context
 *
 * Critical:
 *   Instructions are loaded once at plugin init and cached in memory.
 *   This ensures fast injection without filesystem reads on every message.
 */

import type { Plugin } from "@opencode-ai/plugin"

// ─── Morla Instructions ──────────────────────────────────────────────────────
// Instrucciones maestras del agente Morla - incrustadas directamente en el plugin
// Esto asegura que SIEMPRE estén disponibles sin depender de archivos externos

const MORLA_INSTRUCTIONS = `## 🐢 MORLA AGENT PROTOCOL

⚠️ OBLIGATORIO EN CADA RESPUESTA:

### 1. BÚSQUEDA (SearchKnowledge)
- Ejecuta SIEMPRE antes de responder
- \`SearchKnowledge(term, project: "morla")\` - proyecto OBLIGATORIO
- Sin topic en búsquedas iniciales (búsqueda amplia)
- 3-5 búsquedas con sinónimos diferentes
- CUANDO BUSCAR: Antes de cualquier respuesta, para verificar si existe conocimiento

### 2. CREACIÓN (SetKnowledge) - CUANDO GUARDAR
**GUARDA SIEMPRE que:**
- ✅ Implementes funcionalidad nueva (documentar cómo funciona)
- ✅ Resuelvas un bug (causa raíz + fix)
- ✅ Descubras un patrón o convención nueva
- ✅ Tomes una decisión arquitectónica importante
- ✅ Encuentres un gotcha o aprendizaje

**Topics específicos:**
- \`architecture\` - Cambios estructurales (ej: "Migración a JWT", "Cambio a async/await")
- \`bug\` - Bug descubierto + fix aplicado (ej: "N+1 query en users list")
- \`fix\` - Optimización o mejora de rendimiento (ej: "Caching de embeddings")
- \`convention\` - Estándares/patrones del proyecto (ej: "Naming: CamelCase en métodos")
- \`workflow\` - Procesos/procedimientos (ej: "Release: version → build → push")
- \`discovery\` - Gotchas/aprendizajes (ej: "LocalEmbeddings folder quirk")
- \`requirement\` - Restricciones/requisitos (ej: "JWT include refresh token")

**Flujo:**
1. SearchKnowledge(term, project: "morla") - 3-5 búsquedas amplias
2. Si ENCONTRADO → UpdateKnowledgeById(id, resumen, content)
3. Si NO encontrado → SetKnowledge(topic, title, project: "morla", summary, content)
- Reporta: 🐢 MORLA CREATED {id}

### 3. ACTUALIZACIÓN (UpdateKnowledgeById) - CUANDO ACTUALIZAR
**ACTUALIZA cuando:**
- ✅ Profundices en detalles de una entrada existente
- ✅ Corrijas o amplíes información previamente guardada
- ✅ Encuentres nueva información relacionada
- ✅ Verifiques que el conocimiento sigue siendo válido

**Flujo:**
1. SearchKnowledge encuentra resultado existente
2. UpdateKnowledgeById(id, resumen actualizado, content mejorado)
- Reporta: 🐢 MORLA UPDATED {id}

### 4. SESIONES (SaveSession) - AL TERMINAR SESIÓN
**GUARDA OBLIGATORIAMENTE al finalizar cualquier sesión:**
- Parámetros: project: "morla", topic: "session-summary"
- Title: "Session [fecha] - [descripción breve]"
- Summary: 1-2 líneas de lo que se hizo
- Content estructura:
  ## ✅ COMPLETADO
  - Tarea 1
  - Tarea 2
  
  ## 📝 PENDIENTES
  - Tarea siguiente
  
  ## 🔗 REFERENCIAS CREADAS
  - ID-123456 (descripción)

### 5. PROTOCOLOS MCP
- Ejecuta: \`morla.get_agent_rules()\` al iniciar
- Ejecuta: \`morla.verify_agent_rules()\` para confirmar

---
Última actualización: 2026-04-14`

// ─── Cache ──────────────────────────────────────────────────────────────────

// Instrucciones cacheadas en memoria (ya están aquí, no hay I/O)
const cachedInstructions: string = MORLA_INSTRUCTIONS

// ─── Plugin Export ───────────────────────────────────────────────────────────

export const Morla: Plugin = (ctx) => {
  // Las instrucciones están en memoria (cacheadas como constante)
  const instructions = cachedInstructions

  return {
    // ─── System Prompt Transform ────────────────────────────────
    // Inyecta reglas de Morla en CADA system prompt
    // Mecanismo PRIMARIO para asegurar que el agente SIEMPRE
    // conoce y sigue el protocolo Morla

    "experimental.chat.system.transform": async (_input, output) => {
      const morlaPrompt = `## MORLA AGENT PROTOCOL

${instructions}

---

**IMPLEMENTATION CHECKLIST:**
✅ Read these rules before responding
✅ Use SearchKnowledge before any answer
✅ Save discoveries to SetKnowledge
✅ Update existing entries instead of duplicating
✅ Save session summaries before ending

If you violate these rules, you've violated the Morla protocol.`

      // Append to last system message or create new one
      if (output.system.length > 0) {
        output.system[output.system.length - 1] += "\n\n" + morlaPrompt
      } else {
        output.system.push(morlaPrompt)
      }
    },
  }
}
