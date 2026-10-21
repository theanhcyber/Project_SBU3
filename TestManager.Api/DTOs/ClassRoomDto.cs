using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class ClassRoomDto
{
    public long? Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [Range(1, long.MaxValue)]
    public long SubjectId { get; set; }

    [Range(1, long.MaxValue)]
    public long SemesterId { get; set; }

    [Range(1, long.MaxValue)]
    public long TeacherId { get; set; }

    public string? Code { get; set; }
    public string? SubjectName { get; set; }
    public string? SemesterName { get; set; }
    public string? TeacherName { get; set; }
    public int? StudentCount { get; set; }
}
