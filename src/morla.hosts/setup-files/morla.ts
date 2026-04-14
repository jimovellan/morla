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

const MORLA_INSTRUCTIONS = `## 🐢 REGLAS MAESTRAS DEL AGENTE MORLA

⚠️ ESTAS REGLAS SON INAMOVIBLES Y VINCULANTES - IMPLEMENTA TODAS EN CADA RESPUESTA

### 🔴 PROTOCOLO CRÍTICO

**Ejecuta SIEMPRE al iniciar:**
1. \`morla.get_agent_rules()\` - Obtener reglas actuales
2. \`morla.search_knowledge()\` - Verificar base de conocimiento existente
3. Reporta: 🐢 MORLA [STATUS] al completar cualquier operación

**3-Layer Enforcement System (NO IGNORAR):**
- Capa 1: Cartel - Reglas cargadas en startup
- Capa 2: Plugin - Inyectadas en cada prompt (ESTE ARCHIVO)
- Capa 3: Tool - Disponible como herramienta MCP

### 1️⃣ BÚSQUEDA (SearchKnowledge) - OBLIGATORIO
**SIEMPRE ejecuta ANTES de responder a cualquier pregunta:**
- Parámetro OBLIGATORIO: \`project: "morla"\`
- Sin topic en búsquedas iniciales (búsqueda amplia)
- Realiza 3-5 búsquedas con sinónimos y combinaciones diferentes
- Busca: conceptos, nombres, patrones, palabras clave relacionadas

**Flujo:**
1. SearchKnowledge(searchTerm: "concepto", project: "morla")
2. Si ENCONTRADO → Usa ese conocimiento
3. Si NO → Busca en otras fuentes → GUARDA EN MORLA

### 2️⃣ CREACIÓN (SetKnowledge) - OBLIGATORIO
**Guarda TODOS los descubrimientos nuevos:**

**VALIDACIÓN PREVIA:**
- SearchKnowledge amplias (3-5 búsquedas) para evitar duplicados
- Si existe → UpdateKnowledgeById(id, resumen, content)
- Si NO existe → Crea entrada nueva

**Parámetros obligatorios:**
- \`topic\` - Una de: architecture, bug, fix, convention, workflow, discovery, requirement
- \`title\` - Máximo 6 palabras, descriptivo
- \`project\` - SIEMPRE "morla"
- \`summary\` - Máximo 2-3 líneas (qué, por qué, soluciona)
- \`content\` - Markdown con ## When to Use, ## Content, ## Code Examples

**Reporta:** 🐢 MORLA CREATED {id}

### 3️⃣ ACTUALIZACIÓN (UpdateKnowledgeById) - OBLIGATORIO
**Reutiliza entradas existentes:**
- Parámetro OBLIGATORIO: \`id\` (devuelto por SearchKnowledge o GetKnowledgeById)
- Datos actualizados: resumen + content
- NO DUPLICAR NUNCA
- Reporta: 🐢 MORLA UPDATED {id}

### 4️⃣ SESIONES (SaveSession) - OBLIGATORIO AL TERMINAR
**DEBES guardar resumen al finalizar CUALQUIER sesión:**

**Parámetros:**
- \`project\` - "morla"
- \`topic\` - "session-summary"
- \`title\` - Fecha + breve descripción
- \`summary\` - 1-2 líneas resumiendo qué se hizo
- \`content\` - Markdown con estructura:

\*\*## ✅ QUÉ SE COMPLETÓ
- Tarea 1
- Tarea 2

## 📝 PENDIENTES
- Tarea siguiente
- Validaciones

## 🔗 REFERENCIAS CREADAS
- ID-123456 (descripción)
- ID-234567 (descripción)
\*\*

### 5️⃣ PROTOCOLOS MCP - OBLIGATORIO
- Herramienta requerida: \`morla.get_agent_rules()\` - Ejecuta al iniciar
- Herramienta verificación: \`morla.verify_agent_rules()\` - Confirma que reglas están cargadas
- Respeta siempre: \`initialize\` response del servidor MCP
- Transport: stdio (puerto 6277)

### 📊 TOPICS VÁLIDOS

| Topic | Cuándo Usar | Ejemplo |
|-------|----------|---------|
| \`architecture\` | Decisión estructural | "Cambio a JWT + refresh tokens" |
| \`bug\` | Error descubierto + fix | "Fixed N+1 query en users list" |
| \`fix\` | Solución implementada | "Optimizada búsqueda semántica" |
| \`convention\` | Estándar/patrón | "Naming convention: CamelCase para métodos" |
| \`workflow\` | Proceso/procedimiento | "Release workflow: version → build → publish" |
| \`discovery\` | Gotcha/aprendizaje | "LocalEmbeddings folder location quirk" |
| \`requirement\` | Requisito/restricción | "JWT debe incluir refresh token" |

### ⚡ FORMATO CONTENT TEMPLATE

\`\`\`markdown
## When to Use
[Cuándo/por qué usar esto]

## Content
[Explicación técnica detallada]

## Code Examples
[Fragmentos de código relevantes]

## File Location
[Rutas de archivos importantes]

## Related
[Referencias a otros IDs]
\`\`\`

### 🚫 PROHIBICIONES ABSOLUTAS

❌ NO buscar sin \`project: "morla"\`
❌ NO duplicar conocimiento - BUSCA PRIMERO
❌ NO guardar sin resumir (mín. 2-3 líneas)
❌ NO terminar sesión sin SaveSession
❌ NO ignorar los reportes 🐢 MORLA
❌ NO olvidar ejecutar morla.get_agent_rules() al iniciar

### ✅ CHECKLIST IMPLEMENTACIÓN

Antes de cada respuesta:
- ✅ ¿Ejecuté morla.get_agent_rules()?
- ✅ ¿Busqué en Morla con project: "morla"?
- ✅ ¿Realicé 3-5 búsquedas con sinónimos?
- ✅ ¿Verificaré si existe antes de crear?
- ✅ ¿Reporto el resultado con 🐢 MORLA?

Al terminar:
- ✅ ¿Guardé todas las decisiones?
- ✅ ¿Actualicé entradas existentes?
- ✅ ¿Guardé la sesión con SaveSession?

---
🔐 Protocolo Maestro v1.0
Última actualización: 2026-04-14
Vigencia: Indefinida e inamovible
Autor: Morla System
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
