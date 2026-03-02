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

    ////////////////////////////////////////
    // create
    ////////////////////////////////////////
    [HttpPost]
    public async Task<IActionResult> Create(InvoiceCreateDto dto)
    {
        if (!dto.Rows.Any())
            return BadRequest("Invoice must contain at least one row.");

        var invoice = new Invoice
        {
            CustomerId = dto.CustomerId,
            Comment = dto.Comment,
            Status = InvoiceStatus.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        foreach (var rowDto in dto.Rows)
        {
            invoice.Rows.Add(new InvoiceRow
            {
                Service = rowDto.Service,
                Quantity = rowDto.Quantity,
                Rate = rowDto.Rate,
                Sum = rowDto.Quantity * rowDto.Rate
            });
        }

        invoice.TotalSum = invoice.Rows.Sum(x => x.Sum);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return Ok(Map(invoice));
    }

    ////////////////////////////////////////
    // edit (esli ne otpravlen)
    ////////////////////////////////////////
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, InvoiceCreateDto dto)
    {
        var invoice = await _context.Invoices
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (invoice == null)
            return NotFound();

        if (invoice.Status == InvoiceStatus.Sent)
            return BadRequest("Sent invoices cannot be edited.");

        // remove old rows
        _context.InvoiceRows.RemoveRange(invoice.Rows);

        invoice.CustomerId = dto.CustomerId;
        invoice.Comment = dto.Comment;
        invoice.Rows.Clear();

        foreach (var row in dto.Rows)
        {
            invoice.Rows.Add(new InvoiceRow
            {
                Service = row.Service,
                Quantity = row.Quantity,
                Rate = row.Rate,
                Sum = row.Quantity * row.Rate
            });
        }

        invoice.TotalSum = invoice.Rows.Sum(x => x.Sum);
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(Map(invoice));
    }

    ////////////////////////////////////////
    // change status
    ////////////////////////////////////////
    [HttpPut("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, InvoiceUpdateStatusDto dto)
    {
        var invoice = await _context.Invoices.FindAsync(id);

        if (invoice == null || invoice.DeletedAt != null)
            return NotFound();

        invoice.Status = dto.Status;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    ////////////////////////////////////////
    // soft del (archicve)
    ////////////////////////////////////////
    [HttpDelete("{id}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);

        if (invoice == null || invoice.DeletedAt != null)
            return NotFound();

        invoice.DeletedAt = DateTimeOffset.UtcNow;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    ////////////////////////////////////////
    // hard delete
    ////////////////////////////////////////
    [HttpDelete("{id}/hard")]
    public async Task<IActionResult> HardDelete(int id)
    {
        var invoice = await _context.Invoices
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
            return NotFound();

        if (invoice.Status == InvoiceStatus.Sent)
            return BadRequest("Sent invoice cannot be hard deleted.");

        _context.InvoiceRows.RemoveRange(invoice.Rows);
        _context.Invoices.Remove(invoice);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    ////////////////////////////////////////
    // get all (paginatiob + filter + sort)
    ////////////////////////////////////////
    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? sortBy = "id",
        bool asc = true)
    {
        var query = _context.Invoices
            .Where(x => x.DeletedAt == null)
            .AsQueryable();

        // filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(x =>
                x.Comment!.Contains(search) ||
                x.CustomerId.ToString().Contains(search));
        }

        // sort
        query = sortBy.ToLower() switch
        {
            "total" => asc ? query.OrderBy(x => x.TotalSum) : query.OrderByDescending(x => x.TotalSum),
            "date" => asc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
            _ => asc ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id)
        };

        var total = await query.CountAsync();

        var invoices = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => Map(x))
            .ToListAsync();

        return Ok(new
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Data = invoices
        });
    }

    ////////////////////////////////////////
    // get by id
    ////////////////////////////////////////
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _context.Invoices
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (invoice == null)
            return NotFound();

        return Ok(Map(invoice));
    }

    ////////////////////////////////////////
    // download pdf or docx
    ////////////////////////////////////////
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id, string format = "pdf")
    {
        var invoice = await _context.Invoices
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (invoice == null)
            return NotFound();

        if (format.ToLower() == "pdf")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes("PDF GENERATION HERE");
            return File(bytes, "application/pdf", $"invoice_{id}.pdf");
        }

        if (format.ToLower() == "docx")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes("DOCX GENERATION HERE");
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"invoice_{id}.docx");
        }

        return BadRequest("Invalid format");
    }
    
    private static InvoiceResponseDto Map(Invoice invoice)
    {
        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            TotalSum = invoice.TotalSum,
            Comment = invoice.Comment,
            Status = invoice.Status
        };
    }
}