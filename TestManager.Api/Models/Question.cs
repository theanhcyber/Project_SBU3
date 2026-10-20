namespace TestManager.Api.Models;

public class Question : IEntity
{
    public long Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int? Difficulty { get; set; }

    public long TopicId { get; set; }
    public Topic Topic { get; set; } = null!;

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
}
