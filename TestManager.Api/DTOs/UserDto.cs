using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class UserDto
{
    public long? Id { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? UserName { get; set; }

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? MiddleName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    public string? CurrentPassword { get; set; }

    public bool? Status { get; set; }
}
