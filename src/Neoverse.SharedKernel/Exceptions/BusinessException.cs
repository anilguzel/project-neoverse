namespace Neoverse.SharedKernel.Exceptions;

public class BusinessException : Exception
{
    public string Code { get; }

    public BusinessException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public static class ErrorCodes
{
    public const string NotFound = "not_found";
    public const string ValidationError = "validation_error";
}
