using CRM_API.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService userService)
    {
        _usersService = userService;
    }

    [HttpGet("GetAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _usersService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("GetUsersById/{id}")]
    public async Task<IActionResult> GetUsersById(int id)
    {
        var user = await _usersService.GetUsersByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("CreateUsers")]
    public async Task<IActionResult> CreateUsers([FromBody] Users user)
    {
        var created = await _usersService.CreateUsersAsync(user);
        return CreatedAtAction(nameof(GetUsersById), new { id = created.UserID }, created);
    }

    [HttpPut("UpdateUsersById/{id}")]
    public async Task<IActionResult> UpdateUsersById(int id, [FromBody] Users user)
    {
        if (id != user.UserID) return BadRequest();

        var success = await _usersService.UpdateUsersAsync(user);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpDelete("DeleteUsersById{id}")]
    public async Task<IActionResult> DeleteUsersById(int id)
    {
        var success = await _usersService.DeleteUsersByIdAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}
