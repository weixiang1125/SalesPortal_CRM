using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;

namespace CRM_API.Services
{
    public class UsersService : IUsersService
    {
        private readonly ApplicationDbContext _dbContext;  // Use your DbContext directly
        private readonly IConfiguration _configuration;

        public UsersService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        // Retrieve user by username
        public async Task<Users?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.DBUsers.FirstOrDefaultAsync(u => u.Username == username);
        }

    }
}
