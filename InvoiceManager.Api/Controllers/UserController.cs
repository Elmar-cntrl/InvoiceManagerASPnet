using InvoiceManager.Api.Data;
using InvoiceManager.Api.DTOs.UserDtos;
using InvoiceManager.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }
    /////////////////////////////
    // Register
    /////////////////////////////
    [HttpPost("register")]
    public async Task<ActionResult<UserReturnDto>> Register(UserRegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exist");

        var user = new User
        {
            Name = dto.Name,
            Address = dto.Address,
            Email = dto.Email,
            Password = dto.Password,
            PhoneNumber = dto.PhoneNumber
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new UserReturnDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
    }

    /////////////////////////////
    // Login
    /////////////////////////////
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return Unauthorized("Invalid credentials");

        if (user.Password != dto.Password)
            return Unauthorized("Invalid credentials");

        return Ok(new
        {
            message = "Login successful"
        });
    }

    /////////////////////////////
    // UpdateProfile
    /////////////////////////////
    [HttpPut("profile/{id}")]
    public async Task<IActionResult> UpdateProfile(int id, UserUpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Name = dto.Name;
        user.Address = dto.Address;
        user.PhoneNumber = dto.PhoneNumber;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /////////////////////////////
    //  ChangePassword
    /////////////////////////////
    [HttpPut("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(int id, UserChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (user.Password != dto.CurrentPassword)
            return BadRequest("Current password incorrect");

        user.Password = dto.NewPassword;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}