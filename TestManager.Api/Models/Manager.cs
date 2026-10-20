namespace TestManager.Api.Models;

public class Manager : IEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string Code { get; set; } = string.Empty;

    public long UserId { get; set; }
    public User User { get; set; } = null!;
}
