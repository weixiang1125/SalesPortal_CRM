using CRM_API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Hubs;
using SharedLibrary.Models;
using Task = System.Threading.Tasks.Task;
using TimeHelper = SharedLibrary.Utils.TimeHelper;
public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly WhatsAppService _whatsappService;
    private readonly IHubContext<ChatHub> _hubContext;
    private string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Trim().Replace(" ", "");
        return phone.StartsWith("+") ? phone : "+" + phone;
    }



    public ChatService(ApplicationDbContext context, WhatsAppService whatsappService, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _whatsappService = whatsappService;
        _hubContext = hubContext;
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
        var phone = NormalizePhone(dto.ContactPhone);


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
                CreatedDate = TimeHelper.Now(),
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
            CreatedDate = TimeHelper.Now(),
            Status = "Sent",
            CreatedBy = userId,
            ContactPhone = contact.Phone
        };

        _context.DBChatMessage.Add(message);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(phone).SendAsync("ReceiveMessage", new
        {
            messageId = message.MessageID,
            messageText = dto.MessageText,
            isSender = true,
            createdDate = TimeHelper.Now()
        });


        // 5. (Optional) call WhatsApp API here
        await _whatsappService.SendTextMessage(contact.Phone, dto.MessageText);


        return true;
    }


    public async Task ReceiveWebhookAsync(ReceiveMessageDTO msg)
    {
        var from = NormalizePhone(msg.From);
        var to = NormalizePhone(msg.To);

        var user = await _context.DBUsers.FirstOrDefaultAsync(u => u.Phone == to);
        if (user == null)
        {
            Console.WriteLine("❌ No user found for 'To' phone: " + to);
            return;
        }

        var contact = await _context.DBContacts.FirstOrDefaultAsync(c => c.Phone == from);
        if (contact == null)
        {
            Console.WriteLine("📌 Creating new contact: " + from);
            contact = new Contact
            {
                Phone = from,
                Name = "Unknown " + from,
                CreatedDate = TimeHelper.Now()
            };
            _context.DBContacts.Add(contact);
            await _context.SaveChangesAsync();
        }

        var channel = await _context.DBChatChannel
            .FirstOrDefaultAsync(c => c.UserID == user.UserID && c.ContactID == contact.ContactID);

        if (channel == null)
        {
            Console.WriteLine("📌 Creating new chat channel");
            channel = new ChatChannel
            {
                UserID = user.UserID,
                ContactID = contact.ContactID,
                CreatedDate = TimeHelper.Now(),
                Status = "Active"
            };
            _context.DBChatChannel.Add(channel);
            await _context.SaveChangesAsync();
        }

        var message = new ChatMessage
        {
            ChannelID = channel.ChannelID,
            MessageText = msg.Message,
            MessageType = "text",
            IsSender = false,
            CreatedDate = msg.Timestamp ?? TimeHelper.Now(),
            Status = "Received",
            CreatedBy = user.UserID,
            ContactPhone = from
        };

        Console.WriteLine($"📡 Backend pushing SignalR to group: [{from}]");

        _context.DBChatMessage.Add(message);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(from).SendAsync("ReceiveMessage", new
        {
            messageId = message.MessageID,
            messageText = msg.Message,
            isSender = false,
            createdDate = msg.Timestamp ?? TimeHelper.Now()
        });


    }





}
