using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class SubjectDto
{
    public long? Id { get; set; }

    [Required, StringLength(150)]
    public string SubjectName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;
}
