namespace TestManager.Api.Models;

public class Teacher : IEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TeacherCode { get; set; } = string.Empty; // computed column: TC001
    public bool Status { get; set; }

    public long? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
