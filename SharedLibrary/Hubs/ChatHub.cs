using Microsoft.AspNetCore.SignalR;

namespace SharedLibrary.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"✅ User joined group: {groupName} (conn: {Context.ConnectionId})");
        }


        public async Task SendToGroup(string groupName, object message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }

    }
}
