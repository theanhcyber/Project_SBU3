using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class TopicDto
{
    public long? Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long SubjectId { get; set; }

    [Range(1, long.MaxValue)]
    public long TeacherId { get; set; }

    // Các trường dưới chỉ có khi trả về
    public string? Code { get; set; }
    public string? SubjectName { get; set; }
    public string? TeacherName { get; set; }
    public int? QuestionCount { get; set; }
}
