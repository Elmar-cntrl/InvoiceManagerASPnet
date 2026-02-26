using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.DTOs.UserDtos;

public class UserUpdateProfileDto
{
    [Required]
    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }
}