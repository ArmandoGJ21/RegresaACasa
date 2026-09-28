namespace RegresaACasa.Api.Models;

/// <summary>
/// Resultado de una operación de negocio: el valor, o un mensaje de error que el
/// controlador devuelve como 400 { "success": false, "message": "..." }.
/// </summary>
public sealed class Result<T>
{
    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(string error) => new(default, error);
}
