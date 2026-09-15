// Staj.Application/Common/Models/Result.cs
namespace Staj.Application.Common.Models;

// Standart işlem sonucu modeli
public class Result
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = new();

    public static Result Ok(string? message = null) => new() { Success = true, Message = message };
    public static Result Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? new() };
}

// Veri taşıyan sonuç modeli
public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static new Result<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? new() };
}