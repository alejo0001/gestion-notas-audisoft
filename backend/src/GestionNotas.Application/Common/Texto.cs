using System.Text.RegularExpressions;

namespace GestionNotas.Application.Common;

/// <summary>
/// Normalización de textos antes de guardarlos o compararlos.
/// Evita duplicados "invisibles" como «Samuel  Torres» (dos espacios) frente a «Samuel Torres».
/// Las mayúsculas/minúsculas no se tocan: la intercalación de SQL Server (*_CI_AS) ya las considera iguales.
/// </summary>
public static partial class Texto
{
    /// <summary>Quita los espacios de los extremos y reduce cualquier secuencia de espacios internos a uno solo.</summary>
    public static string Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? string.Empty : EspaciosRepetidos().Replace(valor.Trim(), " ");

    // Regex generada en compilación (source generator): sin costo de interpretación en tiempo de ejecución.
    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosRepetidos();
}
