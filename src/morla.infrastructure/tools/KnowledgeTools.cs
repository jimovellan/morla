using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using Morla.Application.UseCases.Commands.CreateKnowledge;
using Morla.Application.UseCases.Commands.DeleteKnowledge;
using Morla.Application.UseCases.Commands.UpdateKnowledge;
using Morla.Application.UseCases.Queries.SearchKnowledge;
using Morla.Application.UseCases.Queries.GetKnowledgeById;
using Morla.Domain.Repository;
using Serilog;

namespace morla.infrastructure.tools;

[McpServerToolType]
public class KnowledgeTools
{
    private readonly IKnowledgeRepository _knowledgeRepository;
    private readonly ISender _sender;

    public KnowledgeTools(ISender sender, IKnowledgeRepository knowledgeRepository)
    {
        _sender = sender;
        _knowledgeRepository = knowledgeRepository;
    }

[McpServerTool, Description(
    "REGISTRO DE MEMORIA OBLIGATORIO: Ejecuta esta herramienta para PERSISTIR conocimiento clave después de completar una tarea o tomar una decisión. " +
    "Úsala SIEMPRE cuando: " +
    "1. Finalices una funcionalidad o método nuevo (para documentar cómo funciona). " +
    "2. Resuelvas un bug (registra el fix y la causa raíz). " +
    "3. El usuario establezca una nueva regla, preferencia o arquitectura. " +
    "\n\n" +
    "REGLAS DE ESCRITURA: " +
    "- 'title': Debe ser ultra-conciso (máx. 5 palabras). " +
    "- 'summary': Máximo 2 líneas resumiendo el 'qué' y el 'por qué'. " +
    "- 'project': Usa siempre el nombre del proyecto actual. " +
    "- 'topic': Clasifica según la taxonomía: [architecture, bug, fix, convention, workflow, discovery, requirement].")]
public async Task<string> SetKnowledge(
    [Description("Categoría técnica (ej. architecture, bug, fix, convention).")] string topic, 
    [Description("Título descriptivo (Máximo 5 palabras).")] string title, 
    [Description("Nombre del proyecto actual (OBLIGATORIO).")] string project, 
    [Description("Resumen ejecutivo de máximo 2 líneas.")] string summary, 
    [Description("Contenido detallado, código, justificación y detalles técnicos.")] string content)
{
        try
        {
            Log.Information("KnowledgeTools.SetKnowledge: Iniciando creación de entrada...");
            Log.Debug("  - Topic: {Topic}, Title: {Title}, Project: {Project}", topic, title, project);
                        
            Log.Information("KnowledgeTools.SetKnowledge: Enviando comando CreateKnowledge...");
            var result = await _sender.Send(new CreateKnowledgeCommand(topic, title, project, summary, content));

            Log.Information("KnowledgeTools.SetKnowledge: Entrada creada exitosamente con RowId {RowId}", result);
            return $"Knowledge entry created successfully with ID: {result}, RowId: {result}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.SetKnowledge: Error al crear entrada");
            throw;
        }
    }

[McpServerTool, Description(
    "CONSULTA DE MEMORIA OBLIGATORIA: Ejecuta esta herramienta como PRIMER PASO antes de: " +
    "1. Crear cualquier nuevo método, clase o funcionalidad. " +
    "2. Responder preguntas sobre 'cómo se hace algo' o 'qué decidimos'. " +
    "3. Si el usuario dice 'recuerdas', 'existe', 'ayudame' o 'explicame'. " +
    "\n\n" +
    "REGLAS DE OPERACIÓN: " +
    "- Busca SIEMPRE por el 'project' actual para filtrar ruido de otros clientes. " +
    "- Si vas a crear código, busca primero por 'topic' (architecture, convention, fix) para no romper reglas existentes. " +
    "- No asumas que sabes cómo funciona este proyecto; la VERDAD está en esta herramienta. " +
    "\n\n" +
    "TOPICS CLAVE: 'architecture' (diseño), 'convention' (estilo/reglas), 'fix' (soluciones previas), 'bug' (errores conocidos), 'requirement' (negocio).")]
public async Task<List<SearchKnowledgeDto>> SearchKnowledge(
    [Description("Término, palabra clave o concepto a buscar en la memoria.")] string? searchTerm = null, 
    [Description("Categoría específica (ej. architecture, convention, bug, fix).")] string? topic = null, 
    [Description("Nombre del proyecto actual. Es CRÍTICO para obtener resultados relevantes.")] string? project = null, 
    [Description("Cantidad de resultados. Default es 5.")] int limit = 5)

    {
        try
        {
            Log.Information("KnowledgeTools.SearchKnowledge: Iniciando búsqueda...");
            Log.Debug("  - SearchTerm: {SearchTerm}, Topic: {Topic}, Project: {Project}, Limit: {Limit}", searchTerm ?? "null", topic ?? "null", project ?? "null", limit);
            
            var result = await _sender.Send(new SearchKnowledgeQuery(searchTerm, topic, project, limit));
            
            Log.Information("KnowledgeTools.SearchKnowledge: Búsqueda completada. Resultados encontrados: {ResultCount}", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.SearchKnowledge: Error en la búsqueda");
            throw;
        }
    }

    [McpServerTool, Description(
        "RECUPERACIÓN DE ENTRADA COMPLETA: Obtiene todos los detalles (title, summary, content, timestamps, embedding) de una entrada de conocimiento específica. " +
        "Úsala DESPUÉS de SearchKnowledge cuando necesites ver el contenido COMPLETO de una entrada. " +
        "\n\n" +
        "CASOS DE USO COMUNES: " +
        "- El usuario pregunta '¿En qué consiste [concepto]? (después de haberlo buscado). " +
        "- Necesitas revisar detalles técnicos, código o justificación completa. " +
        "- Quieres actualizar una entrada (llama primero GetKnowledgeById, luego UpdateKnowledgeById). " +
        "\n\n" +
        "PARÁMETRO: " +
        "- 'id': El ID devuelto por SearchKnowledge. NO es un número; es un identificador único.")]
    public async Task<GetKnowledgeByIdDto> GetKnowledgeById(
        [Description("ID único de la entrada de conocimiento devuelto por SearchKnowledge. Formato: GUID/hash generado automáticamente.")] string id)
    {
        try
        {
            Log.Information("KnowledgeTools.GetKnowledgeById: Obteniendo entrada con ID {KnowledgeId}", id);
            
            var result = await _sender.Send(new GetKnowledgeByIdQuery(id));
            
            Log.Information("KnowledgeTools.GetKnowledgeById: Entrada obtenida exitosamente");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.GetKnowledgeById: Error al obtener entrada con ID {KnowledgeId}", id);
            throw;
        }
    }

    [McpServerTool, Description(
        "ACTUALIZACIÓN DE CONOCIMIENTO: Modifica el resumen y contenido de una entrada existente. Mantiene el histórico. " +
        "Úsala cuando: (1) Descubres que una regla/decisión cambió, (2) Hay información desactualizada, (3) Necesitas complementar detalles. " +
        "\n\n" +
        "FLUJO RECOMENDADO: " +
        "1. SearchKnowledge(searchTerm: '...', project: currentProject) " +
        "2. GetKnowledgeById(id) para ver contenido actual " +
        "3. UpdateKnowledgeById(id, resumen_nuevo, content_nuevo) " +
        "\n\n" +
        "REGLAS: " +
        "- 'resumen': Debe ser máximo 2-3 líneas con el qué y por qué. " +
        "- 'content': Reemplaza COMPLETAMENTE el contenido anterior; incluye todo lo que quieres conservar. " +
        "- Usa formato Markdown: ##When to Use, ##Content, ##Code Examples. " +
        "- CRÍTICO: Si la entrada es de arquitectura/convención, revisa que los cambios no rompan decisiones existentes.")]
    public async Task<UpdateKnowledgeDto> UpdateKnowledgeById(
        [Description("ID único de la entrada existente. Obtenlo de SearchKnowledge o GetKnowledgeById.")] string id, 
        [Description("Resumen revisado (máximo 2-3 líneas). CLAVE: mejor resumir el cambio, ej. 'Updated: Se agregó soporte para X'.")] string resumen, 
        [Description("Contenido COMPLETO en Markdown. IMPORTANTE: El contenido anterior se REEMPLAZA; incluye TODO lo que quieras conservar.")] string content)
    {
        try
        {
            Log.Information("KnowledgeTools.UpdateKnowledgeById: Actualizando entrada con ID {KnowledgeId}", id);
            Log.Debug("  - Resumen length: {ResumenLength} caracteres", resumen.Length);
            Log.Debug("  - Content length: {ContentLength} caracteres", content.Length);
            
            var result = await _sender.Send(new UpdateKnowledgeCommand(id, resumen, content));
            
            Log.Information("KnowledgeTools.UpdateKnowledgeById: Entrada actualizada exitosamente");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.UpdateKnowledgeById: Error al actualizar entrada con ID {KnowledgeId}", id);
            throw;
        }
    }

    [McpServerTool, Description(
        "ELIMINACIÓN DE CONOCIMIENTO: Elimina permanentemente una entrada de conocimiento por su RowKey único. " +
        "Úsala cuando: (1) Una entrada es incorrecta y debe descartarse, (2) Hay duplicados (mantén el mejor, elimina otros), (3) Información obsoleta. " +
        "\n\n" +
        "REGLAS CRÍTICAS: " +
        "- OPERACIÓN PERMANENTE: No hay recuperación; verifica que sea la entrada correcta antes de llamar. " +
        "- Parámetro 'rowKey': Es el identificador único (GUID generado, ej: 'topic:project:timestamp'). " +
        "- Efecto: Elimina la entrada y sus embeddings asociados de la base de datos. " +
        "\n\n" +
        "FLUJO SEGURO: " +
        "1. SearchKnowledge(searchTerm: ..., project: currentProject) " +
        "2. GetKnowledgeById(id) para confirmar que es la entrada a eliminar " +
        "3. DeleteKnowledgeByRowKey(rowKey) - usa el RowId del paso 2 " +
        "\n\n" +
        "RETORNA: Objeto con success, message, deletedRowKey e info de la eliminación.")]
    public async Task<object> DeleteKnowledgeByRowKey(
        [Description("Identificador único (RowKey) de la entrada a eliminar. Obtenlo de SearchKnowledge o GetKnowledgeById. Ej: 'architecture:morla:2026-04-16T10:30:00Z'.")] string rowKey)
    {
        try
        {
            Log.Information("KnowledgeTools.DeleteKnowledgeByRowKey: Eliminando entrada con RowKey {RowKey}", rowKey);
            
            var command = new DeleteKnowledgeCommand(rowKey);
            var result = await _sender.Send(command);
            
            Log.Information("KnowledgeTools.DeleteKnowledgeByRowKey: Eliminación completada exitosamente {RowKey}", rowKey);
            
            return new
            {
                success = result.Success,
                message = result.Message,
                deletedRowKey = result.DeletedRowKey,
                deletedId = result.DeletedId,
                error = result.Error
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.DeleteKnowledgeByRowKey: Error al eliminar entrada con RowKey {RowKey}", rowKey);
            throw;
        }
    }

    [McpServerTool, Description(
        "REGENERACIÓN DE ÍNDICE SEMÁNTICO: Recalcula los embeddings (búsqueda por similitud semántica) para TODAS las entradas de conocimiento. " +
        "Úsala SOLO en estos casos: " +
        "1. Cambió el modelo de embeddings (LLM diferente, parámetros nuevos). " +
        "2. Se instaló una nueva versión del KnowledgeTools que usa algoritmo de embeddings diferente. " +
        "3. Hay bug en la búsqueda semántica (busca mal conceptos relacionados). " +
        "\n\n" +
        "ADVERTENCIA: " +
        "- Esta operación es PESADA (procesa TODAS las entradas; puede tomar minutos). " +
        "- Usa SOLO durante mantenimiento, NO en producción sin backup. " +
        "- NO necesitas llamarla después de SetKnowledge o UpdateKnowledgeById (se actualiza automáticamente). " +
        "\n\n" +
        "RESULTADO: Retorna mensaje de confirmación. Los embeddings se regeneran en background.")]
    public async Task<string> RegenerateAllEmbeddings()
    {
        try
        {
            Log.Information("KnowledgeTools.RegenerateAllEmbeddings: Iniciando regeneración de todos los embeddings...");
            
            await _knowledgeRepository.RegenerateAllEmbeddingsAsync();
            
            Log.Information("KnowledgeTools.RegenerateAllEmbeddings: ✅ Regeneración completada exitosamente");
            return "All embeddings have been regenerated successfully!";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.RegenerateAllEmbeddings: Error al regenerar embeddings");
            throw;
        }
    }

    [McpServerResource, Description(
        "📚 MANUAL OFICIAL MORLA MCP (instructions://manual-morla-mcp) - Documentación completa con lista de Tools, " +
        "flujos recomendados, parámetros, casos de uso, topics válidos y checklist. " +
        "Copilot lo lee automáticamente al conectar para entender qué capabilities tiene disponibles.")]
    public async Task<string> GetMcpDocumentation()
    {
        try
        {
            Log.Information("KnowledgeTools.GetMcpDocumentation: Generando catálogo de herramientas MCP");
            
            var documentation = @"# MORLA MCP - CATÁLOGO DE HERRAMIENTAS

## 🎯 TOOLS (HERRAMIENTAS EJECUTABLES)

### 1️⃣ SetKnowledge - CREAR ENTRADA (PERSISTENCIA OBLIGATORIA)
**Cuándo usar**: Guardar nueva regla, decisión, bug fix, funcionalidad o aprendizaje
**Parámetros**:
  - topic (string, OBLIGATORIO): architecture | bug | fix | convention | workflow | discovery | requirement
  - title (string, OBLIGATORIO): Título descriptivo (máximo 5 palabras)
  - project (string, OBLIGATORIO): Nombre del proyecto actual
  - summary (string, OBLIGATORIO): Resumen ejecutivo (máximo 2 líneas)
  - content (string, OBLIGATORIO): Contenido Markdown con secciones ##When to Use, ##Content, ##Code Examples
**Retorna**: ID único de la entrada creada
**Ejemplo de flujo**:
  1. Completas una funcionalidad
  2. Llamas: SetKnowledge(topic='architecture', title='JWT auth flow', project='morla', summary='...', content='...')
  3. Recibes ID para futuros updates

---

### 2️⃣ SearchKnowledge - BUSCAR CONOCIMIENTO (BÚSQUEDA OBLIGATORIA ANTES DE CREAR)
**Cuándo usar**: SIEMPRE antes de crear/actualizar (evita duplicados), o cuando necesitas encontrar decisiones/reglas previas
**Parámetros**:
  - searchTerm (string?, OPCIONAL): Palabras clave (ej. 'JWT', 'error handling')
  - topic (string?, OPCIONAL): Filtro por categoría (architecture, bug, fix, etc.)
  - project (string?, CRÍTICO): SIEMPRE incluir proyecto actual para obtener resultados relevantes
  - limit (int, default=5): Máximo de resultados
**Retorna**: Lista de SearchKnowledgeDto con id, title, summary, topic, project
**FLUJO RECOMENDADO**:
  1. SearchKnowledge(searchTerm: 'concepto', project: currentProject)
  2. Si ENCONTRADO → GetKnowledgeById(id) → UpdateKnowledgeById() si necesita cambios
  3. Si NO ENCONTRADO → SetKnowledge() para crear nueva entrada

---

### 3️⃣ GetKnowledgeById - RECUPERAR ENTRADA COMPLETA
**Cuándo usar**: Después de SearchKnowledge cuando necesitas VER detalles completos (content, timestamps)
**Parámetros**:
  - id (string, OBLIGATORIO): ID devuelto por SearchKnowledge
**Retorna**: GetKnowledgeByIdDto con title, summary, content COMPLETO, createdAt, updatedAt, embedding
**Casos de uso**:
  - El usuario pregunta '¿Cómo funciona [concepto]?' → Búsqueda → GetKnowledgeById → Devolver content
  - Necesitas ver contenido anterior antes de actualizar

---

### 4️⃣ UpdateKnowledgeById - ACTUALIZAR ENTRADA EXISTENTE
**Cuándo usar**: La información cambió (regla nueva, bug fix, arquitectura refactorizada)
**Parámetros**:
  - id (string, OBLIGATORIO): ID de entrada existente (de SearchKnowledge)
  - resumen (string, OBLIGATORIO): Resumen actualizado (máximo 2-3 líneas, resumir el cambio)
  - content (string, OBLIGATORIO): Contenido COMPLETO en Markdown (SE REEMPLAZA TODO - incluye lo que quieras conservar)
**Retorna**: UpdateKnowledgeDto con timestamp actualizado
**⚠️ IMPORTANTE**: El content ANTERIOR se pierde; asegúrate de que el nuevo content contiene TODO lo que necesitas
**Flujo seguro**:
  1. GetKnowledgeById(id) → copiar content anterior
  2. UpdateKnowledgeById(id, resumen_nuevo, content_anterior + cambios_nuevos)

---

### 5️⃣ RegenerateAllEmbeddings - REGENERAR ÍNDICE DE BÚSQUEDA SEMÁNTICA
**Cuándo usar**: RARAMENTE - solo en mantenimiento (cambió modelo de embeddings, bug en búsqueda semántica)
**Parámetros**: NINGUNO
**Retorna**: Mensaje de confirmación
**⚠️ ADVERTENCIA**:
  - Operación PESADA (procesa TODAS las entradas, puede tomar minutos)
  - NO llamar después de SetKnowledge/UpdateKnowledgeById (se actualiza automáticamente)
  - Usar SOLO durante mantenimiento con backup

---

## 📚 RESOURCES (CONSULTA - DATOS DE REFERENCIA)

### 1️⃣ GetAvailableTopics - TOPICS VÁLIDOS
**Descripción**: Lista de categorías para clasificar conocimiento
**Cómo leerlo**: Consulta cuando necesites saber qué topics existen
**Contenido**: architecture, bug, fix, convention, workflow, discovery, requirement

### 2️⃣ GetMcpDocumentation - ESTE CATÁLOGO
**Descripción**: Documentación completa (lo que estás leyendo)
**Cómo usarlo**: Copilot lo lee al conectar para entender capabilities y flujos

---

## 🔄 FLUJOS TÍPICOS

### FLUJO 1: Guardar una decisión arquitectónica
```
1. SearchKnowledge(searchTerm: 'decidimos usar JWT', project: 'morla')
   → Si ENCONTRADO: UpdateKnowledgeById()
   → Si NO ENCONTRADO: SetKnowledge(topic='architecture', ...)
```

### FLUJO 2: Documentar un bug fix
```
1. SearchKnowledge(searchTerm: 'N+1 query', topic: 'bug', project: 'morla')
2. Si ENCONTRADO: GetKnowledgeById(id) → ver detalles
3. UpdateKnowledgeById(id, resumen: 'FIXED: ...', content: 'Antes...\n\n## Fix\n...')
```

### FLUJO 3: Consultar reglas antes de implementar
```
1. SearchKnowledge(searchTerm: 'logging pattern', topic: 'convention', project: 'morla')
2. GetKnowledgeById(id) → leer detalles de convención
3. Implementar siguiendo las reglas
```

---

## 📋 TOPICS DISPONIBLES

| Topic | Cuándo | Ejemplo |
|-------|--------|---------|
| **architecture** | Decisiones estructurales | 'JWT + refresh tokens' |
| **bug** | Errores encontrados | 'N+1 query en reports' |
| **fix** | Soluciones a bugs | 'Fixed: Added index on users.email' |
| **convention** | Reglas de código/estilo | 'Logging pattern: Class.Method' |
| **workflow** | Procesos repetidos | 'Release workflow: tag → push → publish' |
| **discovery** | Gotchas/aprendizajes | 'Nullable reference types gotcha' |
| **requirement** | Requisitos negocio | 'Max file size: 250MB' |

---

## 🚀 CHECKLIST PARA COPILOT AL CONECTAR

✅ Leo GetMcpDocumentation al conectar
✅ Sé qué tools tengo disponibles (SetKnowledge, SearchKnowledge, etc.)
✅ Entiendo los flujos recomendados
✅ Sé cuáles parámetros son CRÍTICOS (project, topic, content)
✅ Busco ANTES de crear (evito duplicados)
✅ Leo ANTES de actualizar (no pierdo información)
✅ Categorizo con topics correctos
✅ Guardo URL/línea de código en content

---

Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");

            Log.Information("KnowledgeTools.GetMcpDocumentation: Catálogo generado ({Length} caracteres)", documentation.Length);
            return await Task.FromResult(documentation);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.GetMcpDocumentation: Error al generar documentación");
            throw;
        }
    }

    [McpServerResource, Description(
        "📖 MANUAL DE INICIO - Quick start guide con flujos recomendados, topics válidos, y checklist. " +
        "Copilot lo consulta automáticamente al conectar para entender cómo usar los Tools disponibles.")]
    public async Task<string> GetInstructions()
    {
        try
        {
            Log.Information("KnowledgeTools.GetInstructions: Leyendo archivo de instrucciones");
            
            // Ruta al archivo instructions.md en setup-files
            var baseDir = AppContext.BaseDirectory;
            var instructionsPath = Path.Combine(baseDir, "..", "..", "morla.hosts", "setup-files", "instructions.md");
            var fullPath = Path.GetFullPath(instructionsPath);
            
            if (!File.Exists(fullPath))
            {
                // Intenta ruta alternativa si está instalado como herramienta global
                var altPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Morla", "instructions.md");
                if (File.Exists(altPath))
                {
                    fullPath = altPath;
                }
                else
                {
                    Log.Warning("KnowledgeTools.GetInstructions: Archivo no encontrado en {Path}", fullPath);
                    return "# ⚠️ Instructions not found. Please run 'morla setup' to configure.";
                }
            }
            
            var content = await File.ReadAllTextAsync(fullPath);
            Log.Information("KnowledgeTools.GetInstructions: Instructions leídas ({Length} caracteres)", content.Length);
            return content;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.GetInstructions: Error al leer instrucciones");
            throw;
        }
    }

}

