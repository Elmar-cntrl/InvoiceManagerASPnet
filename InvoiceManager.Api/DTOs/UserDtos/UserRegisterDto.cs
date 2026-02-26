using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.DTOs.UserDtos;

public class UserRegisterDto
{
    [Required]
    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public string? PhoneNumber { get; set; }
}

