namespace TestManager.Api.Models;

public class Exam : IEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float TotalScore { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Code { get; set; } = string.Empty; // computed column: EX001

    public long SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public long TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    public ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();
}
