namespace TestManager.Api.Common.Exceptions;

// Lỗi nghiệp vụ, được AppExceptionHandler chuyển thành ProblemDetails với StatusCode tương ứng.
// Thay cho các chỗ System.out.println(...) / orElseThrow() bên bản Java.
public class AppException(string message, int statusCode = StatusCodes.Status400BadRequest) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public static AppException NotExists(string entity, long id) => new($"{entity} {id} không tồn tại.");

    public static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}
