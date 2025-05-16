using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
