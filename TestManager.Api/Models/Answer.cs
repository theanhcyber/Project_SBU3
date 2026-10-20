namespace TestManager.Api.Models;

public class Answer : IEntity
{
    public long Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public long QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}
