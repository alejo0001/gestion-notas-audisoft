using FluentValidation.Results;

namespace GestionNotas.Application.Common;

internal static class ValidationExtensions
{
    /// <summary>Convierte los errores de FluentValidation al diccionario campo → mensajes (camelCase para el JSON).</summary>
    public static Error ToError(this ValidationResult validationResult) =>
        Error.Validation(validationResult.Errors
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(f => f.ErrorMessage).Distinct().ToArray()));

    private static string ToCamelCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
}
