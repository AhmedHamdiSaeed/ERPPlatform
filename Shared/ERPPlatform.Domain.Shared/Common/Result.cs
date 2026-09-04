using System.Collections.Generic;

namespace ERPPlatform.Common;

public class Result
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public List<string> Errors { get; set; } = new();

    public static Result Ok(string message = "Operation completed successfully.", int statusCode = 200)
    {
        return new Result
        {
            Success = true,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static Result Fail(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new Result
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string> { message }
        };
    }
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Ok(T data, string message = "Operation completed successfully.", int statusCode = 200)
    {
        return new Result<T>
        {
            Success = true,
            Message = message,
            StatusCode = statusCode,
            Data = data
        };
    }

    public static new Result<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new Result<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string> { message },
            Data = default
        };
    }
}
