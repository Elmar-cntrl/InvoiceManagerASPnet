using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.DTOs.CustomerDtos;

public class CustomerCreateDto
{
    [Required]
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}