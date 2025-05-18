using CRM_API.DTOs;
using CRM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Hubs;

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
        public async Task<IActionResult> Receive([FromBody] WhatsAppWebhookPayload payload)
        {
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

            if (msg != null && msg.Type == "text" && contact != null)
            {
                string from = msg.From.StartsWith("+") ? msg.From : "+" + msg.From;
                string text = msg.Text.Body;
                string name = contact.Profile?.Name;
                DateTime created = DateTimeOffset.FromUnixTimeSeconds(long.Parse(msg.Timestamp)).UtcDateTime;

                Console.WriteLine($"📥 From {from} ({name}): {text}");

                // ✅ Create DTO and save
                var dto = new ReceiveMessageDTO
                {
                    From = from,
                    To = metadata?.Display_Phone_Number.StartsWith("+") == true
                        ? metadata.Display_Phone_Number
                        : "+" + metadata.Display_Phone_Number,
                    Message = text,
                    Timestamp = created
                };

                await _chatService.ReceiveWebhookAsync(dto);

                // ✅ Broadcast via SignalR
                await _hubContext.Clients.Group(from).SendAsync("ReceiveMessage", new
                {
                    messageText = text,
                    isSender = false,
                    createdDate = created
                });
            }

            return Ok();
        }

    }
}
