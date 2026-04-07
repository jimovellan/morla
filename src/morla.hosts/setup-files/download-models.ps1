# Script para descargar modelos ONNX después de instalar Morla
# Ejecutado automáticamente por dotnet tool install

$ToolVersion = "0.0.36"
$DownloadUrl = "https://github.com/jimovellan/morla/releases/download/v$ToolVersion/model.onnx_data"
$ToolPath = Split-Path -Parent $PSCommandPath
$ModelsDir = Join-Path $ToolPath "models"
$ModelFile = Join-Path $ModelsDir "model.onnx_data"

# Crear directorio si no existe
if (!(Test-Path $ModelsDir)) {
    New-Item -ItemType Directory -Path $ModelsDir | Out-Null
}

# Si ya existe y tiene tamaño correcto, skip
if (Test-Path $ModelFile) {
    $FileSize = (Get-Item $ModelFile).Length
    if ($FileSize -gt 100000000) {  # Si es > 100MB, asumimos que está bien
        Write-Host "✅ Modelo ONNX ya existe."
        exit 0
    }
}

Write-Host "⏳ Descargando modelo ONNX (este proceso puede tardar varios minutos)..."

try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $ModelFile -UseBasicParsing
    Write-Host "✅ Modelo ONNX descargado exitosamente"
} catch {
    Write-Host "❌ Error al descargar modelo: $_"
    Write-Host "Puedes descargar manualmente desde: $DownloadUrl"
    exit 1
}
