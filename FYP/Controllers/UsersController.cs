using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.DTOs;
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

    [HttpDelete("DeleteUsersById/{id}")]
    public async Task<IActionResult> DeleteUsersById(int id)
    {
        var success = await _usersService.DeleteUsersByIdAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpGet("GetProfile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirst("UserID")?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized("Invalid token");

        var user = await _usersService.GetUsersByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(new UserDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email ?? "",
            Phone = user.Phone ?? ""
        });
    }

    [Authorize]
    [HttpPut("UpdateProfile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserDto dto)
    {
        var userIdStr = User.FindFirst("UserID")?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized("Invalid token");

        var user = await _usersService.GetUsersByIdAsync(userId);
        if (user == null) return NotFound();

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.Phone = dto.Phone;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password); //  hash if changed

        var result = await _usersService.UpdateUsersAsync(user);
        return result ? NoContent() : BadRequest("Update failed");
    }
}
