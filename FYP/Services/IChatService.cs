using SharedLibrary.Models;
using Task = System.Threading.Tasks.Task;

namespace CRM_API.Services
{
    public interface IChatService
    {
        Task<List<ChatMessage>> GetMessagesAsync(int contactId, int userId);
        Task<bool> SendMessageAsync(SendMessageDTO dto, int userId);
        Task<ChatMessage?> ReceiveWebhookAsync(ReceiveMessageDTO dto);
        Task SaveMessageAsync(ChatMessage message);
        Task<string> DownloadMediaAndSaveAsync(string mediaId, string type);


    }
}
