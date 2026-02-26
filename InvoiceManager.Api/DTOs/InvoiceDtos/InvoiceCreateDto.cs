namespace InvoiceManager.Api.DTOs.InvoiceDtos;

public class InvoiceCreateDto
{
    public int CustomerId { get; set; }
    public string? Comment { get; set; }
}