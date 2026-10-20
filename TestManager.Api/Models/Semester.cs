namespace TestManager.Api.Models;

public class Semester : IEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Code { get; set; } = string.Empty; // computed column: SE001

    public ICollection<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();
}
