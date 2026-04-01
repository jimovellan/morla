using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace morla.domain.services;

public static class TrackingKeyHelper
{
    /// <summary>
    /// Genera un slug limpio y corto a partir de un texto.
    /// Quita caracteres especiales y acentos, y recorta si es necesario.
    /// </summary>
    public static string GenerateSlug(string input, int maxLength = 40)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "untitled";

        // Normaliza a minúsculas y elimina acentos
        string normalized = input.ToLowerInvariant();
        normalized = RemoveDiacritics(normalized);

        // Quita caracteres que no sean letras/números/espacios
        normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", "");

        // Reemplaza espacios por guiones y corta a maxLength
        normalized = Regex.Replace(normalized, @"\s+", "-").Trim('-');
        if (normalized.Length > maxLength)
            normalized = normalized.Substring(0, maxLength).Trim('-');

        return normalized;
    }

    /// <summary>
    /// Quita acentos y diacríticos de un texto
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Genera un hash corto (6 caracteres) para evitar colisiones
    /// </summary>
    public static string GenerateShortHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).Substring(0, 6);
    }

    /// <summary>
    /// Genera un trackingKey consistente y legible para bugs/decisiones/features
    /// </summary>
    public static string GenerateTrackingKey(string topic, string proyecto, string titulo)
    {
        // ✅ Normalizar parámetros de entrada (minúsculas, sin acentos)
        var normalizedTopic = GenerateSlug(topic ?? "", 20);
        var normalizedProyecto = GenerateSlug(proyecto ?? "", 20);
        
        // Título corto y limpio
        var slug = GenerateSlug(titulo, 40);

        // Hash corto para estabilidad - usar valores normalizados
        var hash = GenerateShortHash($"{normalizedTopic}|{normalizedProyecto}|{slug}");

        return $"{normalizedTopic}-{normalizedProyecto}-{slug}-{hash}";
    }
}

