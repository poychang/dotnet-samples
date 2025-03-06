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
            // �C��ǰe�@��
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        await HttpContext.Response.CompleteAsync();
        return new EmptyResult();
    }

    [HttpGet("event-stream")]
    public async Task GetEventStream(CancellationToken cancellationToken)
    {
        // �]�w SSE �� Content-Type
        Response.Headers.Append("Content-Type", "text/event-stream");
        // ���s�������֨� SSE �^���A�קK�@���s�����欰�z�Z
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no"); // Nginx ���ϦV�N�z�ɥi��

        await foreach (var item in GetData())
        {
            if (cancellationToken.IsCancellationRequested) break;
            // SSE �ƥ󪺮榡 (�C�Өƥ󳣭n�� "\n\n" ����)
            // event: <event-name>
            // data: <string-data>
            // [�Ŧ�]

            var eventName = "delta";      // �ƥ�W�� (�b DevTools ���|��ܦb "Type" ���)
            var data = new { v = item };  // �A�n�Ǫ���ơA�i�H�O����r��� JSON
            var sseMessage =
                $"event: {eventName}\n" +
                $"data: {data}\n\n";

            await Response.WriteAsync(sseMessage, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            // �C��ǰe�@��
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
