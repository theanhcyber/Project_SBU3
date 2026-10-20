namespace TestManager.Api.Models;

public class Topic : IEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // computed column: TP001

    public long SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public long TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
