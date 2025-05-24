using CRM_API.DTOs;
using CRM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Hubs;
using SharedLibrary.Utils;
using System.Text.Json;

namespace CRM_API.Controllers
{
    [Route("api/webhook/whatsapp")]
    [ApiController]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public WhatsAppWebhookController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Verify()
        {
            var query = Request.Query;

            string mode = query["hub.mode"];
            string challenge = query["hub.challenge"];
            string verifyToken = query["hub.verify_token"];

            if (mode == "subscribe" && verifyToken == "123456")
            {
                return Ok(challenge);
            }

            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            using var reader = new StreamReader(Request.Body);
            var rawJson = await reader.ReadToEndAsync();

            Console.WriteLine("?? RAW WHATSAPP JSON:");
            Console.WriteLine(rawJson);

            WhatsAppWebhookPayload? payload = null;

            try
            {
                payload = JsonSerializer.Deserialize<WhatsAppWebhookPayload>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // ✅ This makes "display_phone_number" match "Display_Phone_Number"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Deserialization error: " + ex.Message);
                return BadRequest();
            }

            if (payload == null)
            {
                Console.WriteLine("❌ Payload is null");
                return BadRequest();
            }

            var msg = payload.Entry?
                .FirstOrDefault()?
                .Changes?
                .FirstOrDefault()?
                .Value?
                .Messages?
                .FirstOrDefault();

            var contact = payload.Entry?
                .FirstOrDefault()?
                .Changes?
                .FirstOrDefault()?
                .Value?
                .Contacts?
                .FirstOrDefault();

            var metadata = payload.Entry?
                .FirstOrDefault()?
                .Changes?
                .FirstOrDefault()?
                .Value?
                .Metadata;

            if (msg != null && contact != null)
            {
                string from = msg.From.StartsWith("+") ? msg.From : "+" + msg.From;
                string name = contact.Profile?.Name;
                DateTime utc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(msg.Timestamp)).UtcDateTime;
                DateTime created = TimeHelper.ConvertToMYT(utc);

                string messageText = "";
                string messageType = msg.Type;

                if (msg.Type == "text")
                {
                    messageText = msg.Text?.Body;
                }
                else if (msg.Type == "image" && msg.Image?.Id != null)
                {
                    Console.WriteLine("📦 Incoming image ID: " + msg.Image?.Id);
                    messageText = await _chatService.DownloadMediaAndSaveAsync(msg.Image.Id, "image");
                }
                else if (msg.Type == "video" && msg.Video?.Id != null)
                {
                    Console.WriteLine("📦 Incoming video ID: " + msg.Video?.Id);
                    messageText = await _chatService.DownloadMediaAndSaveAsync(msg.Video.Id, "video");
                }
                else if (msg.Type == "audio" && msg.Audio?.Id != null)
                {
                    Console.WriteLine("📦 Incoming audio ID: " + msg.Audio?.Id);
                    messageText = await _chatService.DownloadMediaAndSaveAsync(msg.Audio.Id, "audio");
                }
                else if (msg.Type == "document" && msg.Document?.Id != null)
                {
                    Console.WriteLine("📦 Incoming document ID: " + msg.Document?.Id);
                    messageText = await _chatService.DownloadMediaAndSaveAsync(msg.Document.Id, "document");
                }
                else
                {
                    Console.WriteLine($"❌ Unsupported message type: {msg.Type}");
                    return Ok();
                }


                Console.WriteLine($"📥 From {from} ({name}): {messageText}");

                var dto = new ReceiveMessageDTO
                {
                    From = from,
                    To = metadata?.Display_Phone_Number.StartsWith("+") == true
                        ? metadata.Display_Phone_Number
                        : "+" + metadata.Display_Phone_Number,
                    Message = messageText,
                    MessageType = messageType,
                    Timestamp = created
                };

                var savedMsg = await _chatService.ReceiveWebhookAsync(dto);

                if (savedMsg != null)
                {
                    await _hubContext.Clients.Group(from).SendAsync("ReceiveMessage", new
                    {
                        messageId = savedMsg.MessageID,
                        messageText = savedMsg.MessageText,
                        contactPhone = from,
                        isSender = false,
                        createdDate = savedMsg.CreatedDate,
                        timeString = savedMsg.CreatedDate?.ToString("hh:mm tt") ?? "",
                        messageType = savedMsg.MessageType
                    });
                }
            }

            return Ok();
        }


    }
}
