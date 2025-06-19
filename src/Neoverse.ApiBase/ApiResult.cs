namespace Neoverse.ApiBase;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResult<T> Fail(string code, string message) => new() { Success = false, Code = code, Message = message };
}
