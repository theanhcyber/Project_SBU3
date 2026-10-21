namespace TestManager.Api.Models;

public class Student : IEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public bool Status { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();
}
