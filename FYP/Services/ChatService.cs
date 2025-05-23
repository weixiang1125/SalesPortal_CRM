﻿using CRM_API.Services;
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
    private readonly IServiceProvider _serviceProvider;

    private string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Trim().Replace(" ", "");
        return phone.StartsWith("+") ? phone : "+" + phone;
    }

    private string GetMimeType(string ext)
    {
        return ext switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".mp4" => "video/mp4",
            ".ogg" => "audio/ogg",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }


    public ChatService(ApplicationDbContext context, WhatsAppService whatsappService, IHubContext<ChatHub> hubContext, IServiceProvider serviceProvider)
    {
        _context = context;
        _whatsappService = whatsappService;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
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
            contactPhone = phone,
            isSender = true,
            createdDate = message.CreatedDate, //  use the actual saved timestamp
            timeString = message.CreatedDate?.ToString("hh:mm tt") ?? "",
            messageType = message.MessageType
        });

        await _hubContext.Clients.Group(phone).SendAsync("RefreshSidebar", phone);


        // 5. (Optional) call WhatsApp API here
        await _whatsappService.SendMessageAsync(message);

        return true;
    }

    public async Task<string> DownloadMediaAndSaveAsync(string mediaId, string type)
    {
        var urlRes = await _whatsappService.GetMediaUrlAsync(mediaId);
        if (string.IsNullOrEmpty(urlRes)) return "";

        var bytes = await _whatsappService.DownloadBytesAsync(urlRes);
        if (bytes == null || bytes.Length == 0) return "";

        string ext = type switch
        {
            "image" => ".jpg",
            "video" => ".mp4",
            "audio" => ".ogg",
            "document" => ".pdf",
            _ => ".bin"
        };

        string fileName = Guid.NewGuid() + ext;

        // ✅ Upload to Azure Blob
        using var stream = new MemoryStream(bytes);
        var blobService = _serviceProvider.GetRequiredService<IBlobStorageService>();
        var blobUrl = await blobService.UploadFileAsync(stream, fileName, GetMimeType(ext));

        return blobUrl;
    }



    public async Task SaveMessageAsync(ChatMessage message)
    {
        _context.DBChatMessage.Add(message);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(message.ContactPhone).SendAsync("ReceiveMessage", new
        {
            messageId = message.MessageID,
            messageText = message.MessageText,
            contactPhone = message.ContactPhone,
            isSender = message.IsSender,
            createdDate = message.CreatedDate,
            timeString = message.CreatedDate?.ToString("hh:mm tt") ?? "",
            messageType = message.MessageType
        });
    }

    public async Task<ChatMessage?> ReceiveWebhookAsync(ReceiveMessageDTO msg)
    {
        var from = NormalizePhone(msg.From);
        var to = NormalizePhone(msg.To);

        var user = await _context.DBUsers.FirstOrDefaultAsync(u => u.Phone == to);
        if (user == null)
        {
            Console.WriteLine("❌ No user found for 'To' phone: " + to);
            return null;
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
            MessageType = msg.MessageType,
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
            contactPhone = from,
            isSender = false,
            createdDate = message.CreatedDate,
            timeString = message.CreatedDate?.ToString("hh:mm tt") ?? "",
            messageType = message.MessageType
        });


        // ✅ Also notify the user's sidebar group
        string userGroup = $"user-{user.UserID}";
        await _hubContext.Clients.Group(userGroup).SendAsync("RefreshSidebar", from);

        return message;


    }
}
