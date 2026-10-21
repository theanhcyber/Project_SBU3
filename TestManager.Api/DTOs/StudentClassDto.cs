using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class StudentClassDto
{
    public long? Id { get; set; }

    [Range(1, long.MaxValue)]
    public long ClassId { get; set; }

    [Range(1, long.MaxValue)]
    public long StudentId { get; set; }

    public string? ClassName { get; set; }
    public string? ClassCode { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
}
