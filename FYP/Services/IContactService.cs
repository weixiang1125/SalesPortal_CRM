using SharedLibrary.Models;

namespace CRM_API.Services
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetContactsByUserIdAsync(int userId);
        Task<IEnumerable<Contact>> GetAllContactsAsync();
        Task<Contact?> GetContactByIdAsync(int id);
        Task<Contact> CreateContactAsync(Contact contact, int userId);
        Task<bool> UpdateContactAsync(Contact contact, int userId);
        Task<bool> DeleteContactByIdAsync(int id);
        Task<IEnumerable<Contact>> GetAllContactsWithUsersAsync();
        Task<Contact?> GetContactByPhoneAsync(string phone);

    }
}
