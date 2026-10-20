namespace TestManager.Api.Models;

public class User : IEntity
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Status { get; set; } = true;

    public Manager? Manager { get; set; }
    public Teacher? Teacher { get; set; }
    public Student? Student { get; set; }
}
