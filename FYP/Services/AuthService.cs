using Microsoft.IdentityModel.Tokens;
using SharedLibrary;
using SharedLibrary.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRM_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsersService _usersService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(IUsersService usersService, ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _usersService = usersService;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var user = await _usersService.GetUserByUsernameAsync(username);

            if (user == null || !VerifyPassword(password, user.Password))
            {
                return null;
            }

            return GenerateJwtToken(user);
        }

        public async Task<bool> RegisterAsync(string username, string password, string? email, string? phone, string? role)
        {
            var existingUser = await _usersService.GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                return false;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new Users
            {
                Username = username,
                Password = hashedPassword,
                Email = email,
                Phone = phone,
                CreatedDate = DateTime.UtcNow,
                Role = string.IsNullOrEmpty(role) ? "User" : role
            };

            _dbContext.DBUsers.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private bool VerifyPassword(string password, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedPassword);
        }

        public (string Token, DateTime Expiry) GenerateJwtTokenWithExpiry(Users user)
        {
            var expiry = DateTime.Now.AddMinutes(30);
            var token = GenerateJwtToken(user, expiry); // Modified version (see below)
            return (token, expiry);
        }

        // Modified token generator
        private string GenerateJwtToken(Users user, DateTime? customExpiry = null)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: customExpiry ?? DateTime.Now.AddMinutes(30), // Flexible expiry
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
