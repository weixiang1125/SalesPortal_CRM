using SharedLibrary.Models;

namespace CRM_API.Services
{
    public interface IUsersService
    {
        Task<Users?> GetUserByUsernameAsync(string username);  // Get user by username

    }
}
