using SharedLibrary.Models;

namespace CRM_API.Services
{
    public interface IDealService
    {
        Task<IEnumerable<Deal>> GetAllDealAsync();
        Task<IEnumerable<Deal>> GetDealsByUserIdAsync(int userId);

        Task<Deal?> GetDealByIdAsync(int id);
        Task<Deal> CreateDealAsync(Deal deal, int userId);
        Task<bool> UpdateDealAsync(Deal deal, int userId);
        Task<bool> DeleteDealByIdAsync(int id);
    }
}
