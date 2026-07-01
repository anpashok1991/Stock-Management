namespace StockManagement.Application.Common.Models;

public class Result<T>
{
    public bool Succeeded { get; }
    public T? Data { get; }
    public string? Message { get; }
    public List<string> Errors { get; }

    private Result(bool succeeded, T? data, string? message, List<string> errors)
    {
        Succeeded = succeeded;
        Data = data;
        Message = message;
        Errors = errors;
    }

    public static Result<T> Success(T data, string? message = null)
        => new(true, data, message, new());

    public static Result<T> Failure(string error)
        => new(false, default, null, new List<string> { error });

    public static Result<T> Failure(List<string> errors)
        => new(false, default, null, errors);
}

public class Result
{
    public bool Succeeded { get; }
    public string? Message { get; }
    public List<string> Errors { get; }

    private Result(bool succeeded, string? message, List<string> errors)
    {
        Succeeded = succeeded;
        Message = message;
        Errors = errors;
    }

    public static Result Success(string? message = null)
        => new(true, message, new());

    public static Result Failure(string error)
        => new(false, error, new List<string> { error });

    public static Result Failure(List<string> errors)
        => new(false, null, errors);
}
