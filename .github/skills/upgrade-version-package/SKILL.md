---
name: upgrade-version-package
description: Workflow automatizado para liberar/publicar/lanzar nuevas versiones de Morla con versionado, empaquetado NuGet e instalación global. Incluye build Release, creación de paquete, instalación del CLI y registro en git.
keywords: release, version, bump, nuget, package, global-tool, morla, dotnet
applies: morla
---

# 🚀 Skill: Liberar Versión de Morla (.NET Global Tool)

**Versión:** 0.0.1  
**Proyecto:** Morla  
**Propósito:** Workflow automatizado para liberar nuevas versiones  

---

## 📋 Resumen
Automatiza el ciclo completo de liberación de Morla:
1. **Incrementar versión** en 3 .csproj files
2. **Compilar** configuración Release
3. **Empaquetar** NuGet (limpiar antiguos + construir nuevo)
4. **Instalar** herramienta global del sistema
5. **Registrar en git** (commit + tag)

**Cuándo usar:** Después de cambios de código, lista para liberar nueva versión.

---

## 🔧 Workflow Detallado

### Paso 1: Identificar Versión Actual
```bash
grep -r "<Version>" src/**/*.csproj | grep -E "(hosts\.csproj|hosts\.server|hosts\.ui)" 
```
**Archivos a revisar:**
- `src/morla.hosts/morla.hosts.csproj` (versión principal)
- `src/morla.hosts.server/morla.hosts.server.csproj`
- `src/morla.hosts.ui/morla.host.ui.csproj`

### Paso 2: Actualizar Versión (3 archivos simultáneamente)
**Usar:** `multi_replace_string_in_file` (más eficiente que 3 reemplaces separados)

**Patrón:**
```
ANTES: <Version>0.0.55</Version>
AHORA: <Version>0.0.56</Version>
```

**Incluir contexto 3-5 líneas:** facilita búsqueda exacta

### Paso 3: Build Release
```bash
dotnet build -c Release 2>&1 | tail -3
```
**Éxito:** 0 errores, "Compilación correcto"  
**Aceptable:** Warning NU1608 de MediatR

### Paso 4: Pack NuGet
```bash
rm -f nupkg/Morla*.nupkg && \
dotnet pack -c Release src/morla.hosts/morla.hosts.csproj -o nupkg
```
**Verificar:** Archivo `/nupkg/Morla.0.0.56.nupkg` existe  
**Tip:** Usar `isBackground: true` para espera

### Paso 5: Instalar Globalmente
```bash
dotnet tool uninstall --global Morla && \
dotnet tool install --global --add-source /path/to/nupkg Morla
```
**Verificar:** `morla --help` funciona  
**Nota:** Desinstalar primero, esperar, luego instalar

### Paso 6: Git Commit + Tag
```bash
git add -A && git commit -m "chore: bump version to 0.0.56"
git tag v0.0.56
```
**Formato:** `v-MAJOR.MINOR.PATCH`

---

## ✅ Checklist de Calidad

- [ ] Las 3 versiones en .csproj coinciden
- [ ] Build Release: 0 errores
- [ ] NuGet pack ejecutado
- [ ] Herramienta instalada correctamente
- [ ] `morla --help` responde
- [ ] Git commit con mensaje de versión
- [ ] Git tag creado con formato correcto

---

## ⚠️ Decisiones Clave

| Punto | Decisión | Razón |
|-------|----------|-------|
| Multi-replace vs. 3 reemplaces | Multi-replace | 1 operación vs 3, más eficiente |
| Desinstalar antes de instalar | SÍ obligatorio | Evita versión mixta/conflictos |
| Batch versiones en .csproj | SÍ | Coherencia garantizada |
| Compilación en Release | Sí (no Debug) | Optimizado, tamaño menor |

---

## 🔍 Verificación Final

**Después de completar todos los pasos:**

```bash
# 1. Versión globalmente instalada
morla --help

# 2. Git tag existe
git tag | grep v0.0.56

# 3. Paquete en carpeta
ls -lh nupkg/Morla.0.0.56.nupkg
```

---

## 🐛 Solución de Problemas

| Problema | Solución |
|----------|----------|
| Versiones inconsistentes | Verificar con `grep` antes de continuar |
| Build falla | Ejecutar `dotnet clean -c Release` |
| Herramienta sigue versión anterior | Desinstalar primero, esperar, luego instalar |
| Paquete "ya existe" error | Eliminar: `rm -f nupkg/Morla*.nupkg` |
| Git tag duplicado | Usar `--force-with-lease` al push |

---

## 🎯 Prompts de Ejemplo para Usar Esta Skill

1. **"Libera v0.0.61 - usa skill upgrade-version-package"**
2. **"Incrementa versión a 0.0.65 y sigue el workflow de releases"**
3. **"Necesito un nuevo paquete NuGet - usa esta skill"**

---

## 💡 Tips de Eficiencia

- **Paralelizar:** Usar `isBackground: true` en operaciones largas (build, pack)
- **Multi-replace:** Más eficiente que 3 reemplaces separados
- **Esperar terminales:** Usar `await_terminal` antes del siguiente paso
- **Batch versiones:** Actualizar todas a la vez con multi_replace
- **Orden correcto:** Commit/tag al final, después verificar funcionalidad

---

**Última actualización:** 10 de abril 2026  
**Proyecto:** Morla (CLI + MCP + API)