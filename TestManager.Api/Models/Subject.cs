namespace TestManager.Api.Models;

public class Subject : IEntity
{
    public long Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
