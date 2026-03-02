namespace InvoiceManager.Api.DTOs.InvoiceDtos;

public class InvoiceRowCreateDto
{
    public string Service { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
}