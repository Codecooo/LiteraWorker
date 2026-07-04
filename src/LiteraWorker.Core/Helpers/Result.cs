using LiteraWorker.Core.DTO;

namespace LiteraWorker.Core.Helpers;

public readonly record struct Result<T>
{
    public bool Successful { get; }
    public string Message { get; }
    public T? Value { get; }
    public int StatusCode { get; }
    public ProblemDetails? Problem { get; }

    private Result(
        T? value,
        bool success,
        string message,
        int statusCode = default,
        Dictionary<string, string[]>? errors = null,
        ProblemDetails? problem = null)
    {
        Value = value;
        Successful = success;
        Message = message;
        StatusCode = statusCode;
        Problem = problem;
    }

    public static Result<T> Success(T value)
        => new(value, true, message: "Operation successful");

    public static Result<T> Failure(
        string message,
        int statusCode,
        ProblemDetails? problem = null)
        => new(default, false, message, statusCode, null, problem);
}


