using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class QuestionDto
{
    public long? Id { get; set; }

    [Range(1, long.MaxValue)]
    public long TopicId { get; set; }

    // Chỉ có khi trả về
    public string? TopicCode { get; set; }

    [Required, StringLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    [Range(1, 5)]
    public int? Difficulty { get; set; }

    // Ít nhất 2 đáp án, 1 đáp án đúng
    [MinLength(2)]
    public List<AnswerDto> Answers { get; set; } = [];
}
