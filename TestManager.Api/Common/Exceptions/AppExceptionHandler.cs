using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TestManager.Api.Common.Exceptions;

public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        // SaveChanges bọc lỗi SQL trong DbUpdateException, còn ExecuteDelete/ExecuteUpdate ném thẳng SqlException
        var sqlException = exception as SqlException ?? (exception as DbUpdateException)?.InnerException as SqlException;

        var (status, title) = (exception, sqlException) switch
        {
            (AppException e, _) => (e.StatusCode, e.Message),

            // 547: vi phạm khoá ngoại, thường gặp khi xoá bản ghi đang được bản ghi khác tham chiếu
            (_, { Number: 547 }) =>
                (StatusCodes.Status409Conflict, "Không thể thực hiện vì dữ liệu đang được sử dụng ở nơi khác."),

            // 2601/2627: trùng unique key
            (_, { Number: 2601 or 2627 }) =>
                (StatusCodes.Status409Conflict, "Dữ liệu bị trùng."),

            _ => (0, string.Empty)
        };
        if (status == 0) return false;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title }, ct);
        return true;
    }
}
