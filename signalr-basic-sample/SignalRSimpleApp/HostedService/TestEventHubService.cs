using Microsoft.AspNetCore.SignalR;
using SignalRSimpleApp.Hubs;

namespace SignalRSimpleApp.HostedService
{
    /// <summary>
    /// 這個類別用於每 10 秒向 SignalR 客戶端發送資料
    /// </summary>
    /// <param name="eventHub"></param>
    public class TestEventHubService(IHubContext<EventHub> eventHub) : IHostedService
    {
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(SendNow, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;

            void SendNow(object? state)
            {
                var data = DateTime.Now.ToString();
                Console.WriteLine(data);
                eventHub.Clients.All.SendAsync("Receiver", data, cancellationToken).GetAwaiter().GetResult();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
