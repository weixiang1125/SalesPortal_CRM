using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Security.Claims;

namespace CRM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("{contactId}")]
        public async Task<IActionResult> GetMessages(int contactId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("User ID not found in token.");

            var messages = await _chatService.GetMessagesAsync(contactId, userId.Value);
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("User ID not found in token.");

            var result = await _chatService.SendMessageAsync(dto, userId.Value);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("webhook")]
        [AllowAnonymous] // optional: if webhook is public
        public async Task<IActionResult> ReceiveWebhook([FromBody] ReceiveMessageDTO msg)
        {
            await _chatService.ReceiveWebhookAsync(msg);
            return Ok();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string contactPhone)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            if (file.Length > 100 * 1024 * 1024)
                return BadRequest("File too large");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var mime = file.ContentType;

            string type = "file";
            if (mime.StartsWith("image/")) type = "image";
            else if (mime.StartsWith("video/")) type = "video";
            else if (mime.StartsWith("audio/")) type = "audio";
            else if (ext == ".pdf" || ext == ".docx" || ext == ".xlsx") type = "document";

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var uniqueFileName = Guid.NewGuid() + ext;
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var message = new ChatMessage
            {
                ContactPhone = contactPhone,
                MessageText = $"/uploads/{uniqueFileName}",
                MessageType = type,
                CreatedDate = DateTime.UtcNow,
                IsSender = true
            };

            // ✅ Save & push to SignalR
            var dto = new SendMessageDTO
            {
                ContactPhone = contactPhone,
                MessageText = $"/uploads/{uniqueFileName}",
                MessageType = type
            };

            await _chatService.SendMessageAsync(dto, GetUserId().Value);

            // Optional: if you want to return messageId, you can refactor SendMessageAsync to return it
            return Ok(new { url = dto.MessageText, type });
        }


        // 🔐 Helper to safely extract user ID from token
        private int? GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;

            return int.TryParse(userIdClaim.Value, out int id) ? id : null;
        }

    }
}
