using Microsoft.AspNetCore.SignalR;

namespace SignalRSimpleApp.Hubs
{
    /// <summary>
    /// 搭配 <see cref="SignalRSimpleApp.HostedService.TestEventHubService"/> 使用，用來傳送資料給 SignalR 客戶端。"/>
    /// </summary>
    public class EventHub : Hub { }
}
