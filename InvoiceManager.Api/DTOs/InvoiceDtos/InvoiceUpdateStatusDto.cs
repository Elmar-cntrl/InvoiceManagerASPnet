using InvoiceManager.Api.Enums;

namespace InvoiceManager.Api.DTOs.InvoiceDtos;

public class InvoiceUpdateStatusDto
{
    public InvoiceStatus Status { get; set; }
}