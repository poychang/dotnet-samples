using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace StreamResponseWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamController : ControllerBase
{
    private readonly ILogger<StreamController> _logger;

    public StreamController(ILogger<StreamController> logger)
    {
        _logger = logger;
    }

    [HttpGet("binary-stream")]
    public async Task<IActionResult> GetBinaryStream(CancellationToken cancellationToken)
    {
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        HttpContext.Response.StatusCode = 200;

        await HttpContext.Response.StartAsync(cancellationToken);

        await using var stream = HttpContext.Response.BodyWriter.AsStream();
        await foreach (var item in GetData())
        {
            if (item is null) continue;
            var message = JsonSerializer.Serialize(item);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(message), cancellationToken);
            await stream.FlushAsync(cancellationToken);
            // 每秒傳送一次
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        await HttpContext.Response.CompleteAsync();
        return new EmptyResult();
    }

    [HttpGet("event-stream")]
    public async Task GetEventStream(CancellationToken cancellationToken)
    {
        // 設定 SSE 的 Content-Type
        Response.Headers.Append("Content-Type", "text/event-stream");
        // 讓瀏覽器不快取 SSE 回應，避免一些瀏覽器行為干擾
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no"); // Nginx 等反向代理時可用

        await foreach (var item in GetData())
        {
            if (cancellationToken.IsCancellationRequested) break;
            // SSE 事件的格式 (每個事件都要用 "\n\n" 結束)
            // event: <event-name>
            // data: <string-data>
            // [空行]

            var eventName = "delta";      // 事件名稱 (在 DevTools 中會顯示在 "Type" 欄位)
            var data = new { v = item };  // 你要傳的資料，可以是任何字串或 JSON
            var sseMessage =
                $"event: {eventName}\n" +
                $"data: {data}\n\n";

            await Response.WriteAsync(sseMessage, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            // 每秒傳送一次
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        await Response.CompleteAsync();
    }

    private static async IAsyncEnumerable<string> GetData()
    {
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            yield return DateTime.Now.AddDays(i).Date.ToString();
        }
    }
}
