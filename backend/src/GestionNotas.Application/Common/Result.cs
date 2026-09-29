namespace GestionNotas.Application.Common;

public enum ErrorType
{
    NotFound,
    Validation,
    Conflict
}

/// <summary>
/// Error esperado de negocio. <see cref="ValidationErrors"/> solo se llena cuando <see cref="Type"/> es Validation.
/// </summary>
public sealed record Error(
    ErrorType Type,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);

    public static Error Conflict(string message) => new(ErrorType.Conflict, message);

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(ErrorType.Validation, "Se encontraron errores de validación.", errors);

    public static Error Validation(string field, string message) =>
        Validation(new Dictionary<string, string[]> { [field] = [message] });
}

/// <summary>Resultado de una operación que no devuelve valor (por ejemplo, eliminar).</summary>
public class Result
{
    protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);
}

/// <summary>Resultado de una operación que devuelve un valor si fue exitosa.</summary>
public sealed class Result<T> : Result
{
    private Result(T? value, Error? error) : base(error) => Value = value;

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value, null);

    public static new Result<T> Failure(Error error) => new(default, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
