using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.Entities;

public class User
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    [Required]
    [EmailAddress] 
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Customer> Customers { get; set; } = new();
}