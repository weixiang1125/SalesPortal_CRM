using SharedLibrary.Models;

namespace CRM_API.Services
{
    public interface IUsersService
    {
        Task<Users?> GetUserByUsernameAsync(string username);
        Task<Users?> GetUsersByIdAsync(int id);
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users> CreateUsersAsync(Users user);
        Task<bool> UpdateUsersAsync(Users user);
        Task<bool> DeleteUsersByIdAsync(int id);
    }
}
