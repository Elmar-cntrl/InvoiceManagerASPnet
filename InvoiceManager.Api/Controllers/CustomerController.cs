using InvoiceManager.Api.Data;
using InvoiceManager.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManager.Api.DTOs.CustomerDtos;
using InvoiceManager.Api.Enums;

namespace InvoiceManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerController(AppDbContext context)
    {
        _context = context;
    }

    //////////////////////////////////////////
    //  Create customer
    //////////////////////////////////////////
    [HttpPost]
    public async Task<ActionResult<CustomerReturnDto>> Create(CustomerCreateDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Address = dto.Address,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            UserId = 2
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(MapToResponse(customer));
    }

    //////////////////////////////////////////
    // update customer
    //////////////////////////////////////////
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CustomerUpdateDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        customer.Name = dto.Name;
        customer.Address = dto.Address;
        customer.Email = dto.Email;
        customer.PhoneNumber = dto.PhoneNumber;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    //////////////////////////////////////////
    // Hard delete
    //////////////////////////////////////////
    [HttpDelete("{id}")]
    public async Task<IActionResult> HardDelete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        var hasSentInvoice = await _context.Invoices
            .AnyAsync(i => i.CustomerId == id 
                           && i.Status == InvoiceStatus.Sent
                           && i.DeletedAt == null);

        if (hasSentInvoice)
            return BadRequest("Cannot delete customer with sent invoices");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    //////////////////////////////////////////
    // Soft delete 
    //////////////////////////////////////////
    [HttpPut("{id}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        customer.DeletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    //////////////////////////////////////////
    // get list
    //////////////////////////////////////////
    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string sortBy = "id",
        bool asc = true)
    {
        var query = _context.Customers.AsQueryable();

        //////////////////////////////
        // FILTER
        //////////////////////////////
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.Name.Contains(search) ||
                c.Email.Contains(search));
        }

        //////////////////////////////
        // SORT
        //////////////////////////////
        query = sortBy.ToLower() switch
        {
            "name" => asc
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name),

            "email" => asc
                ? query.OrderBy(c => c.Email)
                : query.OrderByDescending(c => c.Email),

            _ => asc
                ? query.OrderBy(c => c.Id)
                : query.OrderByDescending(c => c.Id)
        };

        var total = await query.CountAsync();

        //////////////////////////////
        // PAGINATION
        //////////////////////////////
        var customers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToResponse(c))
            .ToListAsync();

        return Ok(new
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Data = customers
        });
    }

    //////////////////////////////////////////
    // get list by id
    //////////////////////////////////////////
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerReturnDto>> GetById(int id)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == id)
            .Select(c => MapToResponse(c))
            .FirstOrDefaultAsync();

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    //////////////////////////////////////////
    //  маппинг вместо .Select(c => new CustomerResponseDto...
    //////////////////////////////////////////
    private static CustomerReturnDto MapToResponse(Customer c)
    {
        return new CustomerReturnDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            DeletedAt = c.DeletedAt
        };
    }
    
    
}