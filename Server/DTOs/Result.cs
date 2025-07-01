namespace Server.DTOs;

public class Result<T>
{
  public bool IsSuccess { get; init; }
  public T? Data { get; init; }
  public string[] Errors { get; init; } = [];
  public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
  public static Result<T> Failure(params string[] errors) => new() { IsSuccess = false, Errors = errors };
}
