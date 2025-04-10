using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // **GET: /api/user**
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.DBUsers.ToListAsync();
        return Ok(users);
    }

    // **POST: /api/user (Add User)**
    [HttpPost]
    public async Task<IActionResult> AddUser(Users user)
    {
        _context.DBUsers.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUsers), new { id = user.UserID }, user);
    }
}
