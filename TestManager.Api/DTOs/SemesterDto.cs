using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class SemesterDto
{
    public long? Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string? Code { get; set; }
}
