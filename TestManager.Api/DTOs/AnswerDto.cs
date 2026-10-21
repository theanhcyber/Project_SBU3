using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class AnswerDto
{
    public long? Id { get; set; }

    [Required, StringLength(500)]
    public string AnswerText { get; set; } = string.Empty;

    public bool? IsCorrect { get; set; }

    public long? QuestionId { get; set; }
}
