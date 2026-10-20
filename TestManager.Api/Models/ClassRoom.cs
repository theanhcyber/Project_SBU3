namespace TestManager.Api.Models;

// Tên "ClassRoom" giữ như bản Java (tránh trùng từ khoá class), bảng là Classes
public class ClassRoom : IEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Code { get; set; } = string.Empty; // computed column: CL001

    public long SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public long SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;
    public long TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
}
