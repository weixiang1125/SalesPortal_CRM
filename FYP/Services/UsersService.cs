using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;
using Task = System.Threading.Tasks.Task;

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

        // Verify the password against the stored hash
        public async Task<bool> VerifyPasswordAsync(string password, string storedPassword)
        {
            // Here you would use proper hashing (bcrypt, etc.), for example:
            return BCrypt.Net.BCrypt.Verify(password, storedPassword); // If using bcrypt
        }

        public async Task RegisterUserAsync(string username, string password, string email, string phoneNo)
        {
            // Hash the password before saving it
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new Users
            {
                Username = username,
                Password = hashedPassword,
                Email = email,
                Phone = phoneNo

            };

            _dbContext.DBUsers.Add(newUser);
            await _dbContext.SaveChangesAsync();
        }
    }
}
