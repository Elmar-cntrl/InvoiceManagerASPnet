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
    public async Task<ActionResult<List<CustomerReturnDto>>> GetAll()
    {
        var customers = await _context.Customers
            .Select(c => MapToResponse(c))
            .ToListAsync();

        return Ok(customers);
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