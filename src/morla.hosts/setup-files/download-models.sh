#!/bin/bash
# Script para descargar modelos ONNX después de instalar Morla
# Ejecutado automáticamente por dotnet tool install

TOOL_VERSION="0.0.36"
DOWNLOAD_URL="https://github.com/jimovellan/morla/releases/download/v$TOOL_VERSION/model.onnx_data"
TOOL_PATH="$(dirname "$0")"
MODELS_DIR="$TOOL_PATH/models"
MODEL_FILE="$MODELS_DIR/model.onnx_data"

# Crear directorio si no existe
mkdir -p "$MODELS_DIR"

# Si ya existe y tiene tamaño correcto, skip
if [ -f "$MODEL_FILE" ]; then
    FILE_SIZE=$(stat -f%z "$MODEL_FILE" 2>/dev/null || stat -c%s "$MODEL_FILE" 2>/dev/null)
    if [ "$FILE_SIZE" -gt 100000000 ]; then  # Si es > 100MB, asumimos que está bien
        echo "✅ Modelo ONNX ya existe."
        exit 0
    fi
fi

echo "⏳ Descargando modelo ONNX (este proceso puede tardar varios minutos)..."

if command -v curl &> /dev/null; then
    curl -L -o "$MODEL_FILE" "$DOWNLOAD_URL"
    if [ $? -eq 0 ]; then
        echo "✅ Modelo ONNX descargado exitosamente"
    else
        echo "❌ Error al descargar modelo"
        echo "Puedes descargar manualmente desde: $DOWNLOAD_URL"
        exit 1
    fi
elif command -v wget &> /dev/null; then
    wget -O "$MODEL_FILE" "$DOWNLOAD_URL"
    if [ $? -eq 0 ]; then
        echo "✅ Modelo ONNX descargado exitosamente"
    else
        echo "❌ Error al descargar modelo"
        echo "Puedes descargar manualmente desde: $DOWNLOAD_URL"
        exit 1
    fi
else
    echo "❌ ni curl ni wget encontrados"
    exit 1
fi
