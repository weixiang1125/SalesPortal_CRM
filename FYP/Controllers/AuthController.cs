using CRM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsersService _usersService;
        private readonly string _secretKey = "NLGYHV3ja6UCoPOJBq-2ZcStWwQyMhocH_WRxeoKP5w"; // Secret key for signing JWT
        private readonly int _expirationMinutes = 1; // JWT token expiration time

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
            var newToken = GenerateJwtToken(principal.Identity as ClaimsIdentity);

            return Ok(new { Token = newToken });
        }

        // Helper method to get the principal from the expired token
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

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

        // Helper method to generate a new JWT token
        private string GenerateJwtToken(ClaimsIdentity identity)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "SalesPortal",
                audience: "SalesPortal",
                claims: identity.Claims,
                expires: DateTime.Now.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
