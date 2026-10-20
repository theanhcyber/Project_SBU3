using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

// Sinh viên đăng ký thi, trạng thái làm bài, điểm
public class StudentExamDto
{
    public long? Id { get; set; }

    [Range(1, long.MaxValue)]
    public long StudentId { get; set; }

    [Range(1, long.MaxValue)]
    public long ExamId { get; set; }

    // Các trường dưới chỉ có khi trả về
    public float? Mark { get; set; }
    public bool? IsTaken { get; set; }
    public string? StudentName { get; set; }
    public string? ExamName { get; set; }
    public DateTime? ExamStartTime { get; set; }
    public DateTime? ExamEndTime { get; set; }
}
