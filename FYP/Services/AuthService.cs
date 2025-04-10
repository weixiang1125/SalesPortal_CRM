using CRM_API.Services;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IUsersService _usersService;
    private readonly IConfiguration _configuration;

    public AuthService(IUsersService usersService, IConfiguration configuration)
    {
        _usersService = usersService;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _usersService.GetUserByUsernameAsync(username);

        if (user == null || !await _usersService.VerifyPasswordAsync(password, user.Password))
        {
            return null; // Invalid login credentials
        }

        // Generate JWT token if login is successful
        var token = GenerateJwtToken(user);

        return token; // Return the token directly, no need to store it in the user object
    }

    private string GenerateJwtToken(Users user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()), // Use UserID (int)
            //new Claim(ClaimTypes.Role, user.Role) // Include the user's role if necessary
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token); // Return the JWT token as a string
    }

    public async Task<bool> RegisterAsync(string username, string password, string? email, string? phone)
    {
        // Check if user already exists
        var existingUser = await _usersService.GetUserByUsernameAsync(username);
        if (existingUser != null)
        {
            return false;
        }

        // Hash password and register
        await _usersService.RegisterUserAsync(username, password, email, phone);
        return true;
    }


}
