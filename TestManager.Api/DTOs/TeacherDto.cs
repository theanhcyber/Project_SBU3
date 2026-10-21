using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class TeacherDto
{
    public long? Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? TeacherCode { get; set; }

    public bool? Status { get; set; }

    public long? SubjectId { get; set; }
    public string? SubjectName { get; set; }

    public long? UserId { get; set; }

    [Required]
    public UserDto User { get; set; } = new();
}
