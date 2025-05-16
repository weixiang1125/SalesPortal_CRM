using CRM_API.Services;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Models;
using Task = System.Threading.Tasks.Task;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly WhatsAppService _whatsappService;
    public ChatService(ApplicationDbContext context, WhatsAppService whatsappService)
    {
        _context = context;
        _whatsappService = whatsappService;
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(int contactId, int userId)
    {
        var channel = await _context.DBChatChannel
            .FirstOrDefaultAsync(c => c.ContactID == contactId && c.UserID == userId);

        if (channel == null) return new List<ChatMessage>();

        return await _context.DBChatMessage
            .Where(m => m.ChannelID == channel.ChannelID)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> SendMessageAsync(SendMessageDTO dto, int userId)
    {
        // 1. Normalize phone number
        var phone = dto.ContactPhone.StartsWith("+") ? dto.ContactPhone : "+" + dto.ContactPhone;

        // 2. Find Contact
        var contact = await _context.DBContacts.FirstOrDefaultAsync(c => c.Phone == phone);
        if (contact == null)
            return false;

        var contactId = contact.ContactID;

        // 3. Find or create ChatChannel
        var channel = await _context.DBChatChannel
            .FirstOrDefaultAsync(c => c.UserID == userId && c.ContactID == contactId);

        if (channel == null)
        {
            channel = new ChatChannel
            {
                UserID = userId,
                ContactID = contactId,
                CreatedDate = DateTime.UtcNow,
                Status = "Active"
            };
            _context.DBChatChannel.Add(channel);
            await _context.SaveChangesAsync();
        }

        // 4. Create and save ChatMessage
        var message = new ChatMessage
        {
            ChannelID = channel.ChannelID,
            MessageText = dto.MessageText,
            MessageType = dto.MessageType,
            IsSender = true,
            CreatedDate = DateTime.UtcNow,
            Status = "Sent",
            CreatedBy = userId
        };

        _context.DBChatMessage.Add(message);
        await _context.SaveChangesAsync();

        // 5. (Optional) call WhatsApp API here

        return true;
    }


    public async Task ReceiveWebhookAsync(ReceiveMessageDTO msg)
    {
        // 1. Get the user by phone number (your staff)
        var user = await _context.DBUsers.FirstOrDefaultAsync(u => u.Phone == msg.To);
        if (user == null) return; // silently ignore

        // 2. Get or create the contact
        var contact = await _context.DBContacts.FirstOrDefaultAsync(c => c.Phone == msg.From);
        if (contact == null)
        {
            contact = new Contact
            {
                Phone = msg.From,
                Name = "Unknown " + msg.From,
                CreatedDate = DateTime.UtcNow
            };
            _context.DBContacts.Add(contact);
            await _context.SaveChangesAsync();
        }

        // 3. Get or create ChatChannel
        var channel = await _context.DBChatChannel.FirstOrDefaultAsync(c =>
            c.UserID == user.UserID && c.ContactID == contact.ContactID);

        if (channel == null)
        {
            channel = new ChatChannel
            {
                UserID = user.UserID,
                ContactID = contact.ContactID,
                CreatedDate = DateTime.UtcNow,
                Status = "Active"
            };
            _context.DBChatChannel.Add(channel);
            await _context.SaveChangesAsync();
        }

        // 4. Save the incoming message
        var message = new ChatMessage
        {
            ChannelID = channel.ChannelID,
            MessageText = msg.Message,
            MessageType = "text",
            IsSender = false,
            CreatedDate = msg.Timestamp ?? DateTime.UtcNow,
            Status = "Received",
            CreatedBy = user.UserID
        };

        _context.DBChatMessage.Add(message);
        await _context.SaveChangesAsync();
    }



}
