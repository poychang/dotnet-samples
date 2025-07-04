using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapMcp("/mcp");

app.Run("http://localhost:3001");



[McpServerToolType]
public sealed class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"[Remote MCP Server({nameof(Echo)})] {message}";

    [McpServerTool, Description("Echoes in reveres the message.")]
    public static string ReveresEcho(string message) => $"[Remote MCP Server({nameof(ReveresEcho)})] { [.. message.Reverse()]}";
}
