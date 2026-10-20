namespace TestManager.Api.Models;

public class StudentExam : IEntity
{
    public long Id { get; set; }
    public float? Mark { get; set; }
    public bool IsTaken { get; set; }

    // Bài làm: "questionId:answerId:answerId|questionId:answerId" (giữ format như bản Java)
    public string? Answer { get; set; }

    public long ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public long StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
