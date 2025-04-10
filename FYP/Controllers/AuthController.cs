using CRM_API.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace CRM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsersService _usersService;

        public AuthController(IAuthService authService, IUsersService usersService)
        {
            _authService = authService;
            _usersService = usersService;
        }

        // POST api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Delegate the registration logic to the service
            var success = await _authService.RegisterAsync(request.Username, request.Password, request.Email, request.Phone);

            if (!success)
            {
                return BadRequest("Username already exists.");
            }

            return Ok(new { Message = "User registered successfully." });  // Return more structured response
        }

        // POST api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.Username, request.Password);

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new { Token = token });
        }
    }
}
