# 🧠 PROTOCOLO MAESTRO: AGENTE DE DESARROLLO CON MORLA

**Versión:** 1.0  
**Última actualización:** 5 de abril de 2026  
**Servidor MCP:** Morla en puerto 6277 (ejecutar: `dotnet run mcp`)

---

## ⚠️ REGLAS OBLIGATORIAS

### � REGLA #0: REPORTAR SIEMPRE TODAS LAS ACCIONES CON MORLA

**OBLIGATORIO:** Toda operación con Morla DEBE reportarse inmediatamente al usuario con este formato:

```
🔍 MORLA ACTION REPORT
─────────────────────────────────────────
Acción: [SEARCH | CREATE | UPDATE | DELETE]
Término/ID: [lo que buscaste o actualizaste]
Resultado: [FOUND | NOT FOUND | CREATED | UPDATED | ERROR]
Details: [resumen de lo encontrado o creado]
─────────────────────────────────────────
```

### Ejemplos de Reporte Obligatorio

**Ejemplo 1: Búsqueda Exitosa**
```
🔍 MORLA ACTION REPORT
─────────────────────────────────────────
Acción: SEARCH
Término: "JWT authentication"
Resultado: FOUND
Details: Encontrada entrada id=42 'JWT authentication middleware'
         Topic: architecture | Project: morla
─────────────────────────────────────────
```

**Ejemplo 2: Búsqueda Sin Resultados**
```
🔍 MORLA ACTION REPORT
─────────────────────────────────────────
Acción: SEARCH
Término: "OAuth2 implementation"
Resultado: NOT FOUND
Details: No hay entradas sobre OAuth2. Creando nueva...
─────────────────────────────────────────
```

**Ejemplo 3: Entrada Creada**
```
🔍 MORLA ACTION REPORT
─────────────────────────────────────────
Acción: CREATE
ID Generado: 123
Título: "JWT authentication middleware"
Topic: architecture | Project: morla
Resultado: CREATED
Details: Nueva entrada guardada exitosamente
─────────────────────────────────────────
```

**Ejemplo 4: Entrada Actualizada**
```
🔍 MORLA ACTION REPORT
─────────────────────────────────────────
Acción: UPDATE
ID: 42
Título: "JWT authentication middleware"
Resultado: UPDATED
Details: Agregado código de refresh tokens, actualizado resumen
─────────────────────────────────────────
```

**Ejemplo 5: Error en Operación**
```
🔍 MORLA ACTION REPORT
─────────────────────────────────────────
Acción: UPDATE
ID: 999
Resultado: ERROR
Details: Knowledge entry not found (ID inexistente)
─────────────────────────────────────────
```

### ✅ Checklist Obligatorio

ANTES de terminar cualquier operación con Morla, verifica:
- [ ] ¿Realicé una búsqueda? → Reportar resultado (encontrado/no encontrado)
- [ ] ¿Creé una entrada? → Reportar ID generado
- [ ] ¿Actualicé una entrada? → Reportar ID y qué se cambió
- [ ] ¿Hubo error? → Reportar el error exacto
- [ ] ¿El usuario recibió el reporte? → Visible en el chat

---

### �🔴 REGLA #1: SIEMPRE BUSCAR EN MORLA PRIMERO

**Antes de buscar información en cualquier otro lugar (Google, Stack Overflow, YouTube, etc.):**

1. **PRIMERO:** Busca en Morla usando `SearchKnowledge()`
2. **DESPUÉS:** Si no encuentras, busca en otros sitios
3. **AL ENCONTRAR:** Guarda el resultado en Morla para futuras búsquedas

```
Flujo Obligatorio:
┌─────────────────────────────┐
│ ¿Necesito información?      │
└────────┬────────────────────┘
         │
         ▼
┌─────────────────────────────┐
│ BUSCAR EN MORLA PRIMERO     │ ← OBLIGATORIO
│ SearchKnowledge()           │
└────────┬────────────────────┘
         │
         ├─ ✅ Encontrado → USA ESA INFORMACIÓN
         │
         └─ ❌ NO encontrado → BUSCA EN OTROS SITIOS
                              └─ Guarda resultado en Morla
```

**Beneficio:** Construyes una knowledge base propia que acelera futuras búsquedas del equipo.

---

### 📢 REGLA VISUAL: REPORTES DEBEN SER CLAROS Y NOTABLES

Los reportes de Morla DEBEN ser:
- **Visibles:** Usa emojis (🔍, ✅, ❌, ⚠️) para destacar
- **Estructurados:** Sigue el formato de Reporte Obligatorio
- **Inmediatos:** Reporta JUSTO DESPUÉS de cada operación
- **Completos:** Incluye qué se buscó, dónde, qué se encontró, qué se guardó
- **En el chat:** El usuario SIEMPRE ve el resultado de acciones Morla

**NO HAGAS ESTO:**
```
❌ "He buscado información..." (vago, sin detalles)
❌ Silencio completo (el usuario no sabe si buscaste)
❌ Reportar después de 5 operaciones (confuso)
```

**SIEMPRE HAZ ESTO:**
```
✅ Reporte inmediato después de cada acción
✅ Formato estructurado con emojis
✅ ID, título, y resultado claro
✅ Si no se encontró → Informa que crearás nueva entrada
✅ Si actualizaste → Informa qué campos se modificaron
```

---



### Flujo Obligatorio al Guardar Información

**ANTES de crear una NUEVA entrada, SIEMPRE sigue este flujo:**

```
┌──────────────────────────────────┐
│ Tengo información importante     │
│ sobre un topic X                 │
└────────┬─────────────────────────┘
         │
         ▼
    ┌─────────────────┐
    │ ¿Tengo el ID    │
    │ de Morla ya?    │
    └────┬────────┬───┘
         │        │
      SÍ │        │ NO
         ▼        ▼
    ┌──────────────────────┐    ┌────────────────────────┐
    │ UpdateKnowledgeById  │    │ SearchKnowledge       │
    │ Actualizar contenido │    │ Busca tema relacionado│
    │ y resumen            │    └────────┬───────────────┘
    └──────────────────────┘             │
                                         ├─ ✅ Encontrado → Lee el contenido
                                         │   y ACTUALIZA (UpdateKnowledgeById)
                                         │
                                         └─ ❌ No encontrado → Crea NUEVA
                                             entrada (SetKnowledge)
```

### Pasos Detallados

#### **CASO 1: YA TIENES EL ID**
```
1. Tienes: id = "42" (identificador de Morla)
2. Acción: UpdateKnowledgeById(id: "42", resumen: "...", content: "...")
3. Qué hacer: Añade la nueva información al resumen y contenido existente
4. Resultado: Se ACTUALIZA la entrada existente (sin duplicados)
```

#### **CASO 2: NO TIENES EL ID**
```
1. Búsqueda: SearchKnowledge(searchTerm: "JWT authentication", topic: "feature")
2. Si encuentra:
   a) Lee la entrada existente con GetKnowledgeById(id)
   b) Extrae el ID de la entrada
   c) ACTUALIZA con UpdateKnowledgeById (combina tu info + la existente)
   d) Resultado: Sin duplicados, información consolidada

3. Si NO encuentra:
   a) Crea nueva entrada: SetKnowledge(topic, title, project, summary, content)
   b) Guarda el ID que retorna para futuras actualizaciones
   c) Resultado: Nueva entrada en la knowledge base
```

### ✅ Best Practices

- ✅ **SIEMPRE busca antes de crear** - Evita duplicados
- ✅ **Reutiliza IDs** - Mantén entradas consolidadas
- ✅ **Combina información** - Agrupa temas relacionados
- ✅ **Versionado implícito** - UpdatedAt se actualiza automáticamente
- ✅ **Mantén histórico** - El contenido anterior se preserva, solo agregas

---

## 📝 FORMATO OBLIGATORIO DEL CONTENT

### Estructura Markdown Requerida

**Todo `content` DEBE seguir este formato:**

```markdown
## When to Use
[Describe cuándo usar esto, cuándo aplicar, cuándo es relevante]

## Content
[Descripción detallada, explicación técnica, detalles]

## Code Examples (Opcional)
[Si aplica: fragmentos de código, configuración, etc.]

## File Location (Opcional)
[Si aplica: rutas de archivos, ubicación en el proyecto]
```

### Ejemplo Completo

```markdown
## When to Use
Cuando necesitas autenticar usuarios en la aplicación y mantener sesiones 
seguras en arquitecturas distribuidas. Úsalo para APIs REST que requieren
autenticación stateless.

## Content
JWT (JSON Web Tokens) es un estándar abierto que define un método compacto 
y autocontenido para transmitir información de forma segura.

Ventajas:
- Stateless: No requiere almacenamiento en servidor
- Escalable: Funciona en múltiples servidores
- Seguro: Firmado digitalmente

## Code Examples
```csharp
// Generar JWT
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(secretKey);
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[] { new Claim("id", userId) }),
    Expires = DateTime.UtcNow.AddHours(1),
    SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key), 
        SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
```

## File Location
`/src/morla.infrastructure/security/JwtService.cs`
`/src/morla.hosts.server/Program.cs` (configuración)
```

---

### Reglas de Formato

✅ **Encabezados:** Usa `## When to Use`, `## Content`, `## Code Examples`, `## File Location`  
✅ **Código:** Usa bloques de código con lenguaje especificado (csharp, json, sql, etc.)  
✅ **Listas:** Usa `- ` para puntos bullet  
✅ **Énfasis:** Usa `**bold**` para términos importantes  
✅ **Links:** Usa `[texto](url)` si necesitas referencias externas  

❌ **NO:** Mezcles formatos, uses headings `#` (solo `##`), o omitas secciones relevantes

---

## 🎯 Descripción General

Morla es un sistema de gestión de conocimiento integrado en un servidor MCP (Model Context Protocol) que permite:
- ✅ Crear y almacenar entradas de conocimiento
- ✅ Buscar información flexible por término, topic o proyecto
- ✅ Actualizar entradas existentes
- ✅ Regenerar embeddings para búsqueda semántica
- ✅ Integración total con Copilot AI

---

## 🛠️ Herramientas Disponibles

### Estructura de Resúmenes: Claro y Conciso

**El resumen DEBE ser corto y responder 3 preguntas:**
1. **¿Qué es?** - Define brevemente el tema
2. **¿Qué hace?** - Función o acción principal
3. **¿Qué soluciona?** - El problema que resuelve

**Límite:** 2-3 líneas máximo. Evita extensiones innecesarias.

**Ejemplo MALO:**
```
"Este documento describe una implementación detallada de un sistema de autenticación 
que utiliza tokens JWT con refresh tokens para mantener sesiones de usuario seguras 
en una aplicación distribuida, considerando los trade-offs..."
```

**Ejemplo BUENO:**
```
"Implementación de autenticación JWT con refresh tokens. Proporciona sesiones seguras 
y escalables en arquitecturas distribuidas."
```

---

### 1. **SetKnowledge** - Crear entrada nueva
Crea una nueva entrada de conocimiento en la base de datos.

**Parámetros:**
- `topic` (string, requerido): Categoría/tema de la entrada
- `title` (string, requerido): Título descriptivo
- `project` (string, requerido): Proyecto asociado
- `summary` (string, requerido): Resumen corto
- `content` (string, requerido): Contenido completo

**Ejemplo:**
```
SetKnowledge(
  topic="architecture",
  title="JWT authentication middleware",
  project="morla",
  summary="Implementación de autenticación con JWT",
  content="Detalles de implementación..."
)
```

---

### 2. **SearchKnowledge** - Buscar entradas
Busca flexiblemente en la base de conocimiento.

**Parámetros (todos opcionales):**
- `searchTerm`: Término a buscar (busca por palabras individuales)
- `topic`: Filtrar por categoría
- `project`: Filtrar por proyecto

**Scoring automático:**
- Title match: +25 pts
- Topic match: +20 pts
- Summary match: +15 pts
- Content match: +5 pts

**Ejemplos:**
```
// Búsqueda general
SearchKnowledge(searchTerm="JWT authentication")

// Filtrar por topic
SearchKnowledge(searchTerm="embedding", topic="ai")

// Listar todo un topic
SearchKnowledge(topic="architecture")

// Todas las entradas de un proyecto
SearchKnowledge(project="morla")
```

---

### 3. **GetKnowledgeById** - Obtener por ID
Obtiene una entrada completa usando su ID.

**Parámetros:**
- `id` (string): ID único de la entrada

**Ejemplo:**
```
GetKnowledgeById(id="123")
```

---

### 4. **UpdateKnowledgeById** - Actualizar entrada
Actualiza el resumen y contenido de una entrada existente.

**Parámetros:**
- `id` (string): ID de la entrada a actualizar
- `resumen` (string): Nuevo resumen
- `content` (string): Nuevo contenido

**Ejemplo:**
```
UpdateKnowledgeById(
  id="123",
  resumen="JWT con refresh tokens",
  content="Detalles actualizado..."
)
```

---

### 5. **RegenerateAllEmbeddings** - Regenerar embeddings
Regenera todos los embeddings de las entradas. Útil cuando cambias parámetros de embedding.

**Sin parámetros**

**Ejemplo:**
```
RegenerateAllEmbeddings()
```

---

## 📋 Topics Obligatorios y Cuándo Usarlos

### 🔴 **BUG** - Errores y Fixes
**Cuándo usar:** Cuando encuentras y arreglas un error en el código.

**Qué guardar:**
- Descripción del bug (síntomas, impacto)
- Root cause (por qué sucedía)
- Solución implementada
- Líneas de código relevantes
- Cómo prevenirlo en el futuro

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "Fixed N+1 query bug userslist", "CORS error requests cruzadas"

---

### 💡 **FEATURE** - Nuevas Funcionalidades
**Cuándo usar:** Cuando implementas una nueva feature o módulo.

**Qué guardar:**
- Descripción de la feature
- Por qué se implementó
- Cómo funciona (paso a paso)
- Archivos principales involucrados
- Dependencias agregadas

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "User authentication with JWT", "Knowledge search with embeddings"

---

### 🧩 **COMPONENT** - Componentes del Sistema
**Cuándo usar:** Para documentar componentes reutilizables o módulos importantes.

**Qué guardar:**
- Responsabilidad del componente
- Interfaces/contratos
- Dependencias internas
- Ejemplos de uso
- Ubicación en el código

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "KnowledgeRepository abstraction layer", "LocalEmbeddingGenerator ONNX wrapper"

---

### 🏛️ **ARCHITECTURE** - Decisiones Arquitectónicas
**Cuándo usar:** Cuando tomas decisiones que afectan la estructura del proyecto.

**Qué guardar:**
- Problema/contexto
- Alternativas consideradas
- Solución elegida y por qué
- Trade-offs
- Fecha de la decisión

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "Migrado sessions a JWT", "Separado repository layer testability"

---

### ⚙️ **CONFIG** - Configuración y Setup
**Cuándo usar:** Para documentar cómo está configurado el proyecto/ambiente.

**Qué guardar:**
- Variables de entorno
- Valores de configuración
- Archivos de config importantes
- Cómo setupear el ambiente
- Dependencias del sistema

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "ONNX model path config", "Database connection pool setup"

---

### 🎯 **DECISION** - Decisiones Importantes
**Cuándo usar:** Para decisiones que no son arquitectónicas pero sí importantes.

**Qué guardar:**
- La decisión
- Contexto/por qué
- Fechas y responsables
- Impacto

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "Usar Serilog para logging", "Versión 0.0.5 Git LFS"

---

### 📚 **LEARNING** - Aprendizajes y Gotchas
**Cuándo usar:** Después de resolver algo complejo o descubrir algo útil.

**Qué guardar:**
- El problema/pregunta
- Cómo lo resolviste
- Por qué funcionó
- Gotchas/trampas
- Recursos útiles

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "LocalEmbeddings folder path expected", "Git LFS large model files"

---

### 🧪 **TESTING** - Estrategias de Testing
**Cuándo usar:** Cuando defines estrategias, casos de prueba o datos de test.

**Qué guardar:**
- Estrategia de testing
- Casos críticos a probar
- Datos de prueba
- Herramientas usadas
- Coverage esperado

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "Unit test repositories strategy", "Integration test data setup"

---

### ⚡ **PERFORMANCE** - Optimizaciones
**Cuándo usar:** Cuando optimizas código, mejoras velocidad o reduces memory.

**Qué guardar:**
- Métrica original vs mejorada
- Qué se optimizó
- Cómo se logró
- Trade-offs si existen
- Benchmarks

**Formato de título:** Máximo 6 palabras  
**Ejemplo title:** "Embedding search caching optimization", "Chunking embeddings strategy optimized"

---

## 🎯 Flujo de Decisión: Qué Topic Usar

```
¿Es un error que arreglé?
  ↓ SÍ → BUG
  
¿Implementé algo nuevo y funcional?
  ↓ SÍ → FEATURE

¿Es una pieza reutilizable del sistema?
  ↓ SÍ → COMPONENT

¿Decidí cómo debe funcionar la arquitectura?
  ↓ SÍ → ARCHITECTURE

¿Es configuración o setup?
  ↓ SÍ → CONFIG

¿Tomé una decisión importante?
  ↓ SÍ → DECISION

¿Aprendí algo útil o encontré un gotcha?
  ↓ SÍ → LEARNING

¿Creé o definí tests?
  ↓ SÍ → TESTING

¿Optimicé algo?
  ↓ SÍ → PERFORMANCE
```

---

## 🔗 Configuración MCP

**Archivo:** `.vscode/mcp.json`

```json
{
    "servers": {
        "morla": {
            "command": "morla",
            "args": ["mcp"],
            "env": {}
        }
    }
}
```

**Activación:** El servidor MCP se inicia automáticamente cuando usas Copilot en este workspace.

---

## 📂 Estructura de Datos

Cada entrada tiene:
- `id` - ID único (autoincrement)
- `rowId` - GUID para referencia externa
- `topic` - Categoría
- `title` - Título
- `project` - Proyecto asociado
- `summary` - Resumen corto
- `content` - Contenido completo
- `createdAt` - Fecha de creación
- `updatedAt` - Fecha de actualización
- `embedding` - Vector de embeddings (para búsqueda semántica)

---

## 🗄️ Base de Datos

**Ubicación:** `/src/morla.infrastructure/database/`

**Tabla principal:** `knowledges`  
**Tabla embeddings:** `knowledges_embedding`

Las entradas se sincronizaron automáticamente con embeddings ONNX cuando se crean o actualizan.

---

## 🚀 Workflow Recomendado

1. **Descubrir/Investigar** → Usa `SearchKnowledge` para encontrar contexto previo
2. **Documentar** → Usa `SetKnowledge` para guardar nuevos aprendizajes
3. **Actualizar** → Usa `UpdateKnowledgeById` cuando encuentres información nueva
4. **Buscar** → La búsqueda es rápida gracias a embeddings semánticos

---

## 💡 Tips & Best Practices

✅ **Usa títulos descriptivos** - Facilita búsquedas y escaneo rápido  
✅ **Mantén topics consistentes** - Mejora filtrado y organización  
✅ **Resúmenes concisos** - Ideal para búsqueda rápida  
✅ **Contenido detallado** - Incluye ejemplos, contexto, decisiones  
✅ **Actualiza regularmente** - Información fresca = mejor utilidad  

---

## 🔧 Troubleshooting

**Error: "Knowledge entry not found"**
- Verifica el ID usando `SearchKnowledge`
- El ID puede haber cambiado si la entrada fue recreada

**Error: "Failed to regenerate embeddings"**
- Verifica que el modelo ONNX está en `models/onnx/`
- Recompila si hay cambios: `dotnet build morla.sln`

**Búsqueda sin resultados**
- SearchKnowledge busca por palabras individuales, no frases completas
- Prueba términos más genéricos o filtra por topic/project

---

## 📞 Contacto & Soporte

- **Proyecto:** Morla - Knowledge Management System
- **Autor:** Jim
- **Versión actual:** 0.0.5
- **Última compilación:** Release

---

**Última modificación:** 2 de abril de 2026
