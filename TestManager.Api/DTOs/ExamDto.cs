using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class ExamDto
{
    public long? Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public float TotalScore { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [Range(1, long.MaxValue)]
    public long SubjectId { get; set; }

    [Range(1, long.MaxValue)]
    public long TeacherId { get; set; }

    // Các trường dưới chỉ có khi trả về
    public string? Code { get; set; }
    public string? SubjectName { get; set; }
    public string? TeacherName { get; set; }
    public int? QuestionCount { get; set; }

    // Chỉ có khi xem đề (GET /api/exams/{id}/questions), đáp án đúng bị ẩn
    public List<QuestionDto>? Questions { get; set; }
}
