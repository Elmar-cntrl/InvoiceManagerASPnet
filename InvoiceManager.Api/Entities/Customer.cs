using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.Entities;

public class Customer
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}