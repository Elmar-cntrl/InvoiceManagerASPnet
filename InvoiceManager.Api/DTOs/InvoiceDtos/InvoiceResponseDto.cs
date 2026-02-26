using InvoiceManager.Api.Enums;

namespace InvoiceManager.Api.DTOs.InvoiceDtos;

public class InvoiceResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public InvoiceStatus Status { get; set; }
}