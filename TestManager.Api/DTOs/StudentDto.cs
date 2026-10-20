using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class StudentDto
{
    public long? Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    // Tự sinh: ST001...
    public string? StudentCode { get; set; }

    // Bỏ trống khi tạo => true
    public bool? Status { get; set; }

    public long? UserId { get; set; }

    [Required]
    public UserDto User { get; set; } = new();
}
