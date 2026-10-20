namespace TestManager.Api.Models;

public class ExamQuestion : IEntity
{
    public long Id { get; set; }
    public long ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public long QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}
