using SharedLibrary.Models;
using Task = System.Threading.Tasks.Task;

namespace CRM_API.Services
{
    public interface IUsersService
    {
        Task<Users?> GetUserByUsernameAsync(string username);  // Get user by username
        Task<bool> VerifyPasswordAsync(string password, string storedPassword);  // Verify password hash
        Task RegisterUserAsync(string username, string password, string email, string phoneNo);
    }
}
