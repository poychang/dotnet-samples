using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json.Serialization;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    // 使用 NativeAOT 時，要使用有標註型別的序列化選項
    .WithTools<EchoTool>(serializerOptions: new() { TypeInfoResolver = McpToolsJsonContext.Default });
    //.WithToolsFromAssembly(serializerOptions: new() { TypeInfoResolver = McpToolsJsonContext.Default });

await builder.Build().RunAsync();


[McpServerToolType]
public sealed class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"[Local MCP Server({nameof(Echo)})] {message}";

    [McpServerTool, Description("Echoes in reveres the message.")]
    public static string ReveresEcho(string message) => $"[Local MCP Server({nameof(ReveresEcho)})] { [.. message.Reverse()]}";
}

// 使用 NativeAOT 來架構 Local Server 應用程式時，會需要標註序列化會涉及的型別，讓 MCP Server 可以正確序列化和反序列化這些型別。
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(EchoTool))]
internal partial class McpToolsJsonContext : JsonSerializerContext { }

