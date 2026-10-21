using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class StudentDto
{
    public long? Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? StudentCode { get; set; }

    public bool? Status { get; set; }

    public long? UserId { get; set; }

    [Required]
    public UserDto User { get; set; } = new();
}
