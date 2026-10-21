using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class ExamQuestionDto
{
    public long? Id { get; set; }

    public long? ExamId { get; set; }

    [Range(1, long.MaxValue)]
    public long QuestionId { get; set; }
}
