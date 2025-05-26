using CRM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeHelper = SharedLibrary.Utils.TimeHelper;

namespace CRM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsersService _usersService;
        private readonly IConfiguration _configuration;
        private readonly int _expirationMinutes = 30; // JWT token expiration time

        public AuthController(IAuthService authService, IUsersService usersService, IConfiguration configuration)
        {
            _authService = authService;
            _usersService = usersService;
            _configuration = configuration;
        }

        // POST api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Delegate the registration logic to the service
            var success = await _authService.RegisterAsync(request.Username, request.Password, request.Email, request.Phone, request.Role);

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
            if (token == null)
                return Unauthorized(new { message = "Invalid credentials" });


            var user = await _usersService.GetUserByUsernameAsync(request.Username);

            Response.Cookies.Append(
                _configuration["Jwt:CookieName"] ?? "authToken",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = TimeHelper.Now().AddMinutes(_expirationMinutes),
                    Path = "/",
                    IsEssential = true
                });

            return Ok(new { Message = "Login successful", Token = token }); // Include token in the response
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken");
            return Ok(new { Message = "Logged out" });
        }

        // POST api/auth/refresh-token
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            // Validate the old token and refresh it
            var principal = GetPrincipalFromExpiredToken(request.Token);

            if (principal == null)
            {
                return Unauthorized("Invalid or expired token.");
            }

            // Generate a new token based on the claims of the expired token
            var newToken = _authService.GenerateJwtTokenFromClaimsPrincipal(principal);


            return Ok(new { Token = newToken });
        }

        // Helper method to get the principal from the expired token
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);


                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false, // Ignore lifetime validation since we're refreshing
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var securityToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }



    }
}
