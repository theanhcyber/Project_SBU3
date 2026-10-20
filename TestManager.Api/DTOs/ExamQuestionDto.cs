using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

// Câu hỏi trong đề thi
public class ExamQuestionDto
{
    public long? Id { get; set; }

    // Lấy từ URL /api/exams/{examId}/questions, không cần gửi
    public long? ExamId { get; set; }

    [Range(1, long.MaxValue)]
    public long QuestionId { get; set; }
}
