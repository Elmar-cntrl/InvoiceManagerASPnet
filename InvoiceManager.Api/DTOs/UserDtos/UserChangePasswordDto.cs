using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.DTOs.UserDtos;

public class UserChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    public string NewPassword { get; set; } = null!;
}