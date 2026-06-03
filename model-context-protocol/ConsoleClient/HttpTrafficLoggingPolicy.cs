using System.ClientModel.Primitives;

/// <summary>
/// Logging the HTTP traffic
/// </summary>
public class HttpTrafficLoggingPolicy : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        LogRequest(message);
        ProcessNext(message, pipeline, currentIndex);
        LogResponse(message);
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        LogRequest(message);
        await ProcessNextAsync(message, pipeline, currentIndex);
        LogResponse(message);
    }

    private static void LogRequest(PipelineMessage message)
    {
        Console.WriteLine($"\x1b[34m[HTTP Request]\x1b[0m");
        Console.WriteLine($"\x1b[90m{message.Request.Method} {message.Request.Uri}\x1b[0m");
        Console.WriteLine($"\x1b[90m{string.Join('\n', message.Request.Headers.Select(p => $"{p.Key}: {p.Value}"))}\x1b[0m\n");
        if (message.Request.Content != null)
        {
            using var stream = new MemoryStream();
            message.Request.Content.WriteTo(stream, default);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Console.WriteLine($"\x1b[90m{content}\x1b[0m\n");
        }
    }

    private static void LogResponse(PipelineMessage message)
    {
        if (message.Response != null)
        {
            message.Response.BufferContent();
            Console.WriteLine($"\x1b[32m[HTTP Response]\x1b[0m");
            Console.WriteLine($"\x1b[90m{message.Response.Status} {message.Response.ReasonPhrase}\x1b[0m");
            Console.WriteLine($"\x1b[90m{string.Join('\n', message.Request.Headers.Select(p => $"{p.Key}: {p.Value}"))}\x1b[0m\n");
            if (message.Response.Content != null)
            {
                var content = message.Response.Content.ToString();
                Console.WriteLine($"\x1b[90m{content}\x1b[0m\n");
            }
        }
    }
}