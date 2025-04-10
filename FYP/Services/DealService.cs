using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;

namespace CRM_API.Services
{
    public class DealService : IDealService
    {
        private readonly ApplicationDbContext _dbContext;

        public DealService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Deal>> GetAllDealAsync()
        {
            return await _dbContext.DBDeal.Include(d => d.Contact).ToListAsync();
        }

        public async Task<Deal?> GetDealByIdAsync(int id)
        {
            return await _dbContext.DBDeal.Include(d => d.Contact)
                                           .FirstOrDefaultAsync(d => d.DealID == id);
        }

        public async Task<Deal> CreateDealAsync(Deal deal)
        {
            _dbContext.DBDeal.Add(deal);
            await _dbContext.SaveChangesAsync();
            return deal;
        }

        public async Task<bool> UpdateDealAsync(Deal deal)
        {
            _dbContext.DBDeal.Update(deal);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteDealByIdAsync(int id)
        {
            var deal = await _dbContext.DBDeal.FindAsync(id);
            if (deal == null) return false;

            _dbContext.DBDeal.Remove(deal);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
