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

### 2. CREACIÓN (SetKnowledge)
- BUSCA PRIMERO con SearchKnowledge (evita duplicados)
- Topic: architecture | bug | fix | convention | workflow | discovery | requirement
- Parámetros: topic, title (máx 6 palabras), project: "morla", summary (2-3 líneas), content
- Reporta: 🐢 MORLA CREATED {id}

### 3. ACTUALIZACIÓN (UpdateKnowledgeById)
- Si existe entrada → UpdateKnowledgeById(id, resumen, content)
- NO DUPLICAR - busca primero siempre
- Reporta: 🐢 MORLA UPDATED {id}

### 4. SESIONES (SaveSession) - AL TERMINAR
- Parámetros: project: "morla", topic: "session-summary", title, summary, content
- Content: ## ✅ COMPLETADO / 📝 PENDIENTES / 🔗 REFERENCIAS

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
