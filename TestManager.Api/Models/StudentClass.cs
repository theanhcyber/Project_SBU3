namespace TestManager.Api.Models;

public class StudentClass : IEntity
{
    public long Id { get; set; }
    public long ClassId { get; set; }
    public ClassRoom ClassRoom { get; set; } = null!;
    public long StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
