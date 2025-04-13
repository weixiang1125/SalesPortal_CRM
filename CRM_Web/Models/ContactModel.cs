using SharedLibrary.Models;

namespace CRM_Web.Pages.Contacts
{
    public class ContactModel
    {
        public List<Contact> Contacts { get; set; } = new();
        public Contact NewContact { get; set; } = new(); // Add this if it's not there
        public Contact ContactToEdit { get; set; } = new();
    }
}
