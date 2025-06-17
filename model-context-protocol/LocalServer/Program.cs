using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    //.WithTools<EchoTool>()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();



[McpServerToolType]
public sealed class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Hello from Local MCP Server: {message}";

    [McpServerTool, Description("Echoes in reveres the message.")]
    public static string ReveresEcho(string message) => $"{ [.. message.Reverse()]}";
}