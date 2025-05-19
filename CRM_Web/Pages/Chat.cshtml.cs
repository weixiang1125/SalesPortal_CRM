using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Utils;
using System.Net;

namespace CRM_Web.Pages.Chat
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<string> RecentPhones { get; set; } = new();
        public List<ChatMessage> Messages { get; set; } = new();
        public List<Contact> AllContacts { get; set; } = new();

        [BindProperty] public string SelectedPhone { get; set; } = "";
        [BindProperty] public string MessageText { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(string? phone)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            var userId = int.Parse(userIdStr);

            // Load recent contacts from existing channels
            RecentPhones = await _context.DBChatChannel
                .Include(c => c.Contact)
                .Where(c => c.UserID == userId && c.Contact.Phone != null)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => c.Contact.Phone!)
                .Distinct()
                .ToListAsync();

            // All contacts for modal
            AllContacts = await _context.DBContacts.OrderBy(c => c.Name).ToListAsync();

            // ?? Clean and normalize the selected phone
            var raw = WebUtility.HtmlDecode(phone ?? RecentPhones.FirstOrDefault() ?? string.Empty)
                .Replace(" ", "").Trim();

            SelectedPhone = raw.TrimStart('+');
            SelectedPhone = "+" + SelectedPhone;

            // 1. Try to find or create contact
            var contact = await _context.DBContacts.FirstOrDefaultAsync(c => c.Phone == SelectedPhone);
            if (contact == null)
            {
                contact = new Contact
                {
                    Phone = SelectedPhone,
                    Name = "Unknown " + SelectedPhone,
                    CreatedDate = TimeHelper.Now()
                };
                _context.DBContacts.Add(contact);
                await _context.SaveChangesAsync();
            }

            // 2. Try to find or create ChatChannel
            var channel = await _context.DBChatChannel
                .FirstOrDefaultAsync(c => c.UserID == userId && c.ContactID == contact.ContactID);

            if (channel == null)
            {
                channel = new ChatChannel
                {
                    UserID = userId,
                    ContactID = contact.ContactID,
                    CreatedDate = TimeHelper.Now(),
                    Status = "Active"
                };
                _context.DBChatChannel.Add(channel);
                await _context.SaveChangesAsync();
            }

            // 3. Load messages
            Messages = await _context.DBChatMessage
                .Where(m => m.ChannelID == channel.ChannelID)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetMessagesAsync(string phone)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr))
                return new JsonResult(new List<ChatMessage>());

            var userId = int.Parse(userIdStr);

            // Normalize phone again
            var normalizedPhone = WebUtility.HtmlDecode(phone ?? "").Replace(" ", "").Trim();
            normalizedPhone = normalizedPhone.TrimStart('+');
            normalizedPhone = "+" + normalizedPhone;

            // Get correct channel
            var channelId = await _context.DBChatChannel
                .Where(c => c.UserID == userId && c.Contact.Phone == normalizedPhone)
                .Select(c => c.ChannelID)
                .FirstOrDefaultAsync();

            if (channelId == 0)
                return new JsonResult(new List<object>());

            // Return JSON with matching property names for JS
            var messages = await _context.DBChatMessage
                .Where(m => m.ChannelID == channelId)
                .OrderBy(m => m.CreatedDate)
                .Select(m => new
                {
                    messageText = m.MessageText,
                    isSender = m.IsSender,
                    createdDate = m.CreatedDate
                })
                .ToListAsync();

            return new JsonResult(messages);
        }
    }
}
