using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Utils;

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
            return await _dbContext.DBDeal
                .Include(d => d.Contact)
                .Include(d => d.CreatedByUser)
                .Include(d => d.UpdatedByUser)
                .ToListAsync();
        }

        public async Task<Deal?> GetDealByIdAsync(int id)
        {
            return await _dbContext.DBDeal
                .Include(d => d.Contact)
                .Include(d => d.CreatedByUser)
                .Include(d => d.UpdatedByUser)
                .FirstOrDefaultAsync(d => d.DealID == id);
        }

        public async Task<IEnumerable<Deal>> GetDealsByUserIdAsync(int userId)
        {
            return await _dbContext.DBDeal
                .Include(d => d.Contact)
                .Include(d => d.CreatedByUser)
                .Include(d => d.UpdatedByUser)
                .Where(d => d.CreatedBy == userId)
                .ToListAsync();
        }

        public async Task<Deal> CreateDealAsync(Deal deal, int userId)
        {
            deal.CreatedDate = TimeHelper.Now();
            deal.CreatedBy = userId;

            _dbContext.DBDeal.Add(deal);
            await _dbContext.SaveChangesAsync();
            return deal;
        }

        public async Task<bool> UpdateDealAsync(Deal deal, int userId)
        {
            // Check if the deal exists in the database before attempting to update
            var existingDeal = await _dbContext.DBDeal.FindAsync(deal.DealID);
            if (existingDeal == null)
            {
                // Deal not found, return false
                return false;
            }

            // Update the properties of the existing deal
            existingDeal.DealName = deal.DealName;
            existingDeal.Value = deal.Value;
            existingDeal.Stage = deal.Stage;
            existingDeal.Status = deal.Status;
            existingDeal.ExpectedCloseDate = deal.ExpectedCloseDate;
            existingDeal.ContactID = deal.ContactID;

            existingDeal.UpdatedDate = TimeHelper.Now();
            existingDeal.UpdatedBy = userId;

            // Save the changes
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
