using Microsoft.AspNetCore.SignalR;

namespace SignalRSimpleApp.Hubs
{
    /// <summary>
    /// 用於模擬聊天室，處理多個客戶端的訊息
    /// </summary>
    public class ChatHub : Hub
    {
        public async Task Send(string name, string message)
        {
            await Clients.All.SendAsync("broadcastMessage", name, message);
        }
    }
}
