# 🧠 PROTOCOLO MAESTRO: AGENTE DE DESARROLLO CON MORLA

**Versión:** 1.0  
**Última actualización:** 5 de abril de 2026  
**Servidor MCP:** Morla en puerto 6277 (ejecutar: `dotnet run mcp`)

---

## ⚠️ REGLAS OBLIGATORIAS

### 🔴 REGLA #1: SIEMPRE BUSCAR EN MORLA PRIMERO

**Antes de buscar información en cualquier otro lugar (Google, Stack Overflow, YouTube, etc.):**

1. **PRIMERO:** Busca en Morla usando `SearchKnowledge()`
2. **DESPUÉS:** Si no encuentras, busca en otros sitios
3. **AL ENCONTRAR:** Guarda el resultado en Morla para futuras búsquedas

### 🔴 REGLA #2: EVITAR DUPLICADOS (Upsert Inteligente)

```
if (tengo_id_morla) → UpdateKnowledgeById()
else → SearchKnowledge()
       if (encontrado) → GetKnowledgeById() + UpdateKnowledgeById()
       else → SetKnowledge()
```

---

## 📋 9 TOPICS OBLIGATORIOS

1. **BUG** → Errores + root cause + solución
2. **FEATURE** → Nueva funcionalidad + cómo funciona
3. **COMPONENT** → Módulo reutilizable + interfaces
4. **ARCHITECTURE** → Decisión estructural + trade-offs
5. **CONFIG** → Configuración + setup
6. **DECISION** → Decisión importante + contexto
7. **LEARNING** → Gotcha/aprendizaje
8. **TESTING** → Estrategia de tests
9. **PERFORMANCE** → Optimización + métricas

---

## 📝 FORMATO CONTENT (Markdown Obligatorio)

```markdown
## When to Use
[Cuándo, escenarios, triggers]

## Content
[Descripción técnica completa]

## Code Examples (Opcional)
[Fragmentos de código]

## File Location (Opcional)
[Rutas relativas al proyecto]
```

---

## 📋 FORMATO RESÚMENES (CRÍTICO)

- **Máximo:** 2-3 líneas
- **Estructura:** ¿Qué es? ¿Qué hace? ¿Qué soluciona?

✅ BUENO: "Autenticación JWT. Proporciona sesiones seguras en arquitecturas distribuidas."

---

## 🔄 FLUJO OBLIGATORIO

1. **INICIO:** GetLastSession() ← contexto previo
2. **CONSULTA:** SearchKnowledge(searchTerm, topic, project="morla")
3. **PERSISTENCIA:** Upsert (update si existe, create si no)
4. **CIERRE:** SaveSession(tópicos, lo_hecho, pendientes)

---

## 🛠️ HERRAMIENTAS (4)

- `SetKnowledge(topic, title, project, summary, content)`
- `SearchKnowledge(searchTerm?, topic?, project)`
- `GetKnowledgeById(id)`
- `UpdateKnowledgeById(id, resumen, content)`

---

## ✅ BEST PRACTICES

✅ Títulos cortos (máx 6 palabras)
✅ Topics consistentes
✅ Resúmenes 2-3 líneas
✅ Markdown estructurado
✅ Busca SIEMPRE antes de crear
✅ Proyecto OBLIGATORIO
✅ Actualiza regularmente
✅ No duplices: usa flujo upsert

---

**Instalado con:** `morla setup`
