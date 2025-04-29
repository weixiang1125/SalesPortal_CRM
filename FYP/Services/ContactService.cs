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
        public async Task<IEnumerable<Contact>> GetContactsByUserIdAsync(int userId)
        {
            return await _dbContext.DBContacts
                .Where(c => c.CreatedBy == userId)
                .ToListAsync();
        }


        public async Task<Contact> CreateContactAsync(Contact contact, int userId)
        {
            contact.CreatedDate = DateTime.UtcNow;
            contact.CreatedBy = userId;  // Make sure to set the CreatedBy user
            _dbContext.DBContacts.Add(contact);
            await _dbContext.SaveChangesAsync();
            return contact;
        }

        public async Task<bool> UpdateContactAsync(Contact contact, int userId)
        {
            var existingContact = await _dbContext.DBContacts.FindAsync(contact.ContactID);
            if (existingContact == null) return false;

            existingContact.Name = contact.Name;
            existingContact.Email = contact.Email;
            existingContact.Phone = contact.Phone;
            existingContact.Company = contact.Company;
            existingContact.Notes = contact.Notes;
            existingContact.Status = contact.Status;
            existingContact.UpdatedDate = DateTime.UtcNow;
            existingContact.UpdatedBy = userId;

            await _dbContext.SaveChangesAsync();
            return true;
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
