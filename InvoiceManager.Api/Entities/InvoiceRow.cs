using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Api.Entities;

public class InvoiceRow
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    [Required]
    public string Service { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Sum { get; set; }
}