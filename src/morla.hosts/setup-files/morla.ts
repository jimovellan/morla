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

const MORLA_INSTRUCTIONS = `## REGLAS MAESTRAS DEL AGENTE MORLA

⚠️ ESTAS REGLAS SON INAMOVIBLES - IMPLEMENTA TODAS EN CADA RESPUESTA

### 1. BÚSQUEDA (SearchKnowledge) - OBLIGATORIO
- SIEMPRE busca primero en Morla ANTES de responder
- Parámetro OBLIGATORIO: \`project: "morla"\`
- Si NO está en Morla → busca en otras fuentes → GUARDA EN MORLA
- No uses Topic en las busquedas
- realiza de 3 a 5 busquedas por cada pregunta usando diferentes sinonimos o combinaciones

### 2. CREACIÓN (SetKnowledge) - OBLIGATORIO
- SIEMPRE buscar previamente con SearchKnowledge para evitar duplicados, usando busquedas amplias sin topic y sinonimos reiterando de 3 a 5 veces
- Cuando descubres algo nuevo → guardar en Morla
- Topics válidos: architecture, bug, fix, convention, workflow, discovery, requirement
- SIEMPRE incluir ejemplos de código

### 3. ACTUALIZACIÓN (UpdateKnowledgeById) - OBLIGATORIO
- Si existe entrada → ACTUALIZAR, NO DUPLICAR
- Devuelve ID para futuras referencias
- Busca primero con SearchKnowledge

### 4. SESIONES (SaveSession) - OBLIGATORIO AL TERMINAR
- Al finalizar sesión: guardar resumen
- Formato: ✅ COMPLETADO / 📝 PENDIENTES / 🔗 REFERENCIAS

### 5. PROTOCOLOS MCP - OBLIGATORIO
- Ejecuta \`morla.get_agent_rules()\` al iniciar sesión
- Lee TODOS los prompts de Morla
- Respeta objeto \`initialize\` del servidor MCP

---
Autor: Morla Protocol
Vigencia: Indefinida
Última actualización: 2026-04-14
`

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
