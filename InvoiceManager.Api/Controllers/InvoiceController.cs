using InvoiceManager.Api.Data;
using InvoiceManager.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManager.Api.DTOs.InvoiceDtos;
using InvoiceManager.Api.Enums;

namespace InvoiceManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly AppDbContext _context;

    public InvoiceController(AppDbContext context)
    {
        _context = context;
    }
    
    /////////////////////////////////////////////
    //  Create
    /////////////////////////////////////////////
    [HttpPost]
    public async Task<IActionResult> Create(InvoiceCreateDto dto)
    {
        var invoice = new Invoice
        {
            CustomerId = dto.CustomerId,
            Comment = dto.Comment,
            Status = InvoiceStatus.Created
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return Ok(invoice);
    }

    /////////////////////////////////////////////
    // UpdateStatus
    /////////////////////////////////////////////
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, InvoiceUpdateStatusDto dto)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        invoice.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /////////////////////////////////////////////
    //  Delete
    /////////////////////////////////////////////
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /////////////////////////////////////////////
    //  GetAll
    /////////////////////////////////////////////
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _context.Invoices
            .Select(i => new InvoiceResponseDto
            {
                Id = i.Id,
                CustomerId = i.CustomerId,
                TotalSum = i.TotalSum,
                Comment = i.Comment,
                Status = i.Status
            })
            .ToListAsync();

        return Ok(invoices);
    }

    /////////////////////////////////////////////
    //  GetById
    /////////////////////////////////////////////
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _context.Invoices
            .Where(i => i.Id == id)
            .Select(i => new InvoiceResponseDto
            {
                Id = i.Id,
                CustomerId = i.CustomerId,
                TotalSum = i.TotalSum,
                Comment = i.Comment,
                Status = i.Status
            })
            .FirstOrDefaultAsync();

        if (invoice == null)
            return NotFound();

        return Ok(invoice);
    }
}