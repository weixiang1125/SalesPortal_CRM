using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;

namespace CRM_API.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _dbContext;

        public ContactService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Contact>> GetAllContactsAsync()
        {
            return await _dbContext.DBContacts.ToListAsync();
        }

        public async Task<Contact?> GetContactByIdAsync(int id)
        {
            return await _dbContext.DBContacts.FindAsync(id);
        }

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            contact.CreatedDate = DateTime.UtcNow;
            _dbContext.DBContacts.Add(contact);
            await _dbContext.SaveChangesAsync();
            return contact;
        }

        public async Task<bool> UpdateContactAsync(Contact contact)
        {
            _dbContext.DBContacts.Update(contact);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteContactByIdAsync(int id)
        {
            var contact = await _dbContext.DBContacts.FindAsync(id);
            if (contact == null) return false;

            _dbContext.DBContacts.Remove(contact);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
