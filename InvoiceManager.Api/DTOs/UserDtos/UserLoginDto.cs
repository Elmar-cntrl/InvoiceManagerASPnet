using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.DTOs.UserDtos;

public class UserLoginDto
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}