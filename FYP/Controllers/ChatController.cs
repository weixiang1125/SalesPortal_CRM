using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IWebHostEnvironment _env;
        public ChatController(IChatService chatService, IWebHostEnvironment env)
        {
            _chatService = chatService;
            _env = env;
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
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string contactPhone, [FromServices] IBlobStorageService blobService)
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

            string blobUrl;

            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            blobUrl = await blobService.UploadFileAsync(file.OpenReadStream(), uniqueFileName, mime);


            var dto = new SendMessageDTO
            {
                ContactPhone = contactPhone,
                MessageText = blobUrl,
                MessageType = type
            };

            await _chatService.SendMessageAsync(dto, GetUserId().Value);
            return Ok(new { url = blobUrl, type });
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
