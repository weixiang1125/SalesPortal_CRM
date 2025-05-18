using Microsoft.AspNetCore.SignalR;

namespace SharedLibrary.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendToGroup(string groupName, string messageJson)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", messageJson);
        }
    }
}
