using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

// Dùng chung cho tạo / sửa / trả về tài khoản và đổi mật khẩu (giống UserDTO bên Java).
// Bắt buộc nhập Email, UserName, FirstName, LastName (kiểm tra trong UserService vì đổi mật khẩu không cần).
public class UserDto
{
    public long? Id { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? UserName { get; set; }

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? MiddleName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    // Chỉ dùng khi gửi lên: mật khẩu lúc tạo tài khoản / mật khẩu mới khi đổi mật khẩu. Không bao giờ trả về
    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    // Chỉ dùng khi đổi mật khẩu
    public string? CurrentPassword { get; set; }

    public bool? Status { get; set; }
}
