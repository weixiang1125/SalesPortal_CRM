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
        public Dictionary<string, ChatMessage?> LastMessagesByPhone { get; set; } = new();

        [BindProperty] public string SelectedPhone { get; set; } = "";
        [BindProperty] public string MessageText { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(string? phone)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            var userId = int.Parse(userIdStr);

            // Load recent contact phones from ChatChannel
            RecentPhones = await _context.DBChatChannel
                .Include(c => c.Contact)
                .Where(c => c.UserID == userId && c.Contact.Phone != null)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => c.Contact.Phone!)
                .Distinct()
                .ToListAsync();

            // Load all contacts for modal
            AllContacts = await _context.DBContacts.OrderBy(c => c.Name).ToListAsync();

            // Normalize and ensure only ONE "+" and no spaces
            var raw = WebUtility.HtmlDecode(phone ?? RecentPhones.FirstOrDefault() ?? string.Empty)
                .Replace(" ", "")
                .Trim();

            SelectedPhone = raw.StartsWith("+") ? raw : "+" + raw;


            // Ensure the contact exists
            var contact = await _context.DBContacts.FirstOrDefaultAsync(c => c.Phone == SelectedPhone);
            if (contact == null)
            {
                contact = new Contact
                {
                    Phone = SelectedPhone,
                    Name = "Unknown " + SelectedPhone,
                    CreatedDate = TimeHelper.Now(),
                    CreatedBy = userId
                };
                _context.DBContacts.Add(contact);
                await _context.SaveChangesAsync();
            }

            // Ensure the channel exists
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

            // Load all messages for this chat
            Messages = await _context.DBChatMessage
                .Where(m => m.ChannelID == channel.ChannelID)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();

            // Prepare last message preview for every recent phone
            foreach (var phoneNum in RecentPhones)
            {
                var normalized = phoneNum.Trim();

                var contactId = await _context.DBContacts
                    .Where(c => c.Phone == normalized)
                    .Select(c => c.ContactID)
                    .FirstOrDefaultAsync();

                if (contactId == 0) continue;

                var chanId = await _context.DBChatChannel
                    .Where(c => c.UserID == userId && c.ContactID == contactId)
                    .Select(c => c.ChannelID)
                    .FirstOrDefaultAsync();

                if (chanId == 0) continue;

                var lastMsg = await _context.DBChatMessage
                    .Where(m => m.ChannelID == chanId)
                    .OrderByDescending(m => m.CreatedDate)
                    .FirstOrDefaultAsync();

                LastMessagesByPhone[normalized] = lastMsg;
            }

            return Page();
        }

        public async Task<JsonResult> OnGetSidebarAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr))
                return new JsonResult(new { success = false, error = "Not logged in" });

            var userId = int.Parse(userIdStr);

            // Reload latest contacts from ChatChannel
            var recentPhones = await _context.DBChatChannel
                .Include(c => c.Contact)
                .Where(c => c.UserID == userId && c.Contact.Phone != null)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => c.Contact.Phone!)
                .Distinct()
                .ToListAsync();

            var lastMessages = new List<object>();

            foreach (var phoneNum in recentPhones)
            {
                var normalized = phoneNum.Trim();

                var contactId = await _context.DBContacts
                    .Where(c => c.Phone == normalized)
                    .Select(c => c.ContactID)
                    .FirstOrDefaultAsync();

                if (contactId == 0) continue;

                var chanId = await _context.DBChatChannel
                    .Where(c => c.UserID == userId && c.ContactID == contactId)
                    .Select(c => c.ChannelID)
                    .FirstOrDefaultAsync();

                if (chanId == 0) continue;

                var lastMsg = await _context.DBChatMessage
                    .Where(m => m.ChannelID == chanId)
                    .OrderByDescending(m => m.CreatedDate)
                    .FirstOrDefaultAsync();

                int unreadCount = await _context.DBChatMessage
                    .Where(m => m.ChannelID == chanId && m.IsSender == false && m.IsRead == false)
                    .CountAsync();

                string groupLabel = "";
                if (lastMsg?.CreatedDate != null)
                {
                    var msgDate = TimeHelper.ConvertToMYT(lastMsg.CreatedDate.Value).Date;
                    var today = TimeHelper.Now().Date;
                    var yesterday = today.AddDays(-1);

                    groupLabel = msgDate == today ? "Today"
                               : msgDate == yesterday ? "Yesterday"
                               : msgDate.ToString("MMMM dd, yyyy");
                }

                lastMessages.Add(new
                {
                    phone = normalized,
                    text = lastMsg?.MessageText,
                    date = lastMsg?.CreatedDate,
                    unreadCount,
                    group = groupLabel
                });
            }

            return new JsonResult(new
            {
                success = true,
                phones = recentPhones,
                lastMessages
            });
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

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> OnPostMarkAsReadAsync([FromBody] string phone)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            phone = WebUtility.HtmlDecode(phone).Replace(" ", "").Trim();
            phone = phone.TrimStart('+');
            phone = "+" + phone;

            var channelId = await _context.DBChatChannel
                .Where(c => c.UserID == userId && c.Contact.Phone == phone)
                .Select(c => c.ChannelID)
                .FirstOrDefaultAsync();

            if (channelId == 0) return NotFound();

            var unreadMessages = await _context.DBChatMessage
                .Where(m => m.ChannelID == channelId && m.IsSender == false && m.IsRead == false)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, updated = unreadMessages.Count });
        }

    }
}
