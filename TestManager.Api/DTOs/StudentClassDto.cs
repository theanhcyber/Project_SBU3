using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

// Sinh viên trong lớp
public class StudentClassDto
{
    public long? Id { get; set; }

    [Range(1, long.MaxValue)]
    public long ClassId { get; set; }

    [Range(1, long.MaxValue)]
    public long StudentId { get; set; }

    // Các trường dưới chỉ có khi trả về
    public string? ClassName { get; set; }
    public string? ClassCode { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
}
