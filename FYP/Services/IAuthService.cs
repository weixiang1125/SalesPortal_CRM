using System.Security.Claims;

namespace CRM_API.Services
{
    public interface IAuthService
    {
        // Method to log in the user and return a JWT token (nullable string)
        Task<string?> LoginAsync(string username, string password);
        string GenerateJwtTokenFromClaimsPrincipal(ClaimsPrincipal principal);


        // Method to register a new user and return a success flag (true/false)
        Task<bool> RegisterAsync(string username, string password, string? email, string? phone, string? role);
    }
}
