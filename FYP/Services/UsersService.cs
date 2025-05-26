using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;
namespace CRM_API.Services
{
    public class UsersService : IUsersService
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Users?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.DBUsers
                .FirstOrDefaultAsync(u => u.Username == username);
        }


        public async Task<Users?> GetUsersByIdAsync(int id)
        {
            return await _dbContext.DBUsers.FindAsync(id);
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            return await _dbContext.DBUsers.ToListAsync();
        }

        public async Task<Users> CreateUsersAsync(Users user)
        {
            _dbContext.DBUsers.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UpdateUsersAsync(Users user)
        {
            _dbContext.DBUsers.Update(user);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUsersByIdAsync(int id)
        {
            var user = await _dbContext.DBUsers.FindAsync(id);
            if (user == null)
                return false;

            _dbContext.DBUsers.Remove(user);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
