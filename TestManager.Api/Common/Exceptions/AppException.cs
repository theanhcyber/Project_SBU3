namespace TestManager.Api.Common.Exceptions;

public class AppException(string message, int statusCode = StatusCodes.Status400BadRequest) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public static AppException NotExists(string entity, long id) => new($"{entity} {id} không tồn tại.");

    public static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}
