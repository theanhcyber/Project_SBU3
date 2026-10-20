using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class ManagerDto
{
    public long? Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;

    // Bỏ trống khi tạo => true
    public bool? Status { get; set; }

    public long? UserId { get; set; }

    [Required]
    public UserDto User { get; set; } = new();
}
