using SharedLibrary.Models;

namespace CRM_API.Services
{
    public interface IDealService
    {
        Task<IEnumerable<Deal>> GetAllDealAsync();
        Task<Deal?> GetDealByIdAsync(int id);
        Task<Deal> CreateDealAsync(Deal deal);
        Task<bool> UpdateDealAsync(Deal deal, int userId);
        Task<bool> DeleteDealByIdAsync(int id);
    }
}
