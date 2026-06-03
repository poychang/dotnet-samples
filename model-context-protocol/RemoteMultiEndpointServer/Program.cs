using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithListToolsHandler(async (context, ct) =>
    {
        var services = context.Services ?? throw new InvalidOperationException("Service provider is unavailable.");
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        var serverId = httpContextAccessor
            .HttpContext?
            .Request
            .RouteValues["serverId"]?
            .ToString();
        var visibleTools = serverId switch
        {
            "a1" => new[] { McpServerTool.Create(A1Tools.A1) },
            "a2" => new[] { McpServerTool.Create(A2Tools.A2) },
            _ => []
        };

        return new ListToolsResult
        {
            Tools = [.. visibleTools.Select(t => t.ProtocolTool)]
        };
    })
    .WithCallToolHandler(async (context, ct) =>
    {
        var services = context.Services ?? throw new InvalidOperationException("Service provider is unavailable.");
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        var serverId = httpContextAccessor
            .HttpContext?
            .Request
            .RouteValues["serverId"]?
            .ToString();
        var visibleTools = serverId switch
        {
            "a1" => new[] { McpServerTool.Create(A1Tools.A1) },
            "a2" => new[] { McpServerTool.Create(A2Tools.A2) },
            _ => []
        };
        var allowedToolNames = visibleTools.Select(p => p.ProtocolTool.Name);
        var requestedToolName = context.Params.Name;

        if (!allowedToolNames.Contains(requestedToolName))
        {
            throw new McpProtocolException(
                $"Tool '{requestedToolName}' is not available on endpoint '{serverId}'.",
                McpErrorCode.InvalidRequest);
        }

        var tool = visibleTools.Single(t => t.ProtocolTool.Name == requestedToolName);

        return await tool.InvokeAsync(context, ct);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapMcp("/mcp/{serverId}");

app.Run("https://localhost:3001");



[McpServerToolType]
public sealed class A1Tools
{
    [McpServerTool(Name = "A1"), Description("Echoes A1 calling.")]
    public static string A1(string input) => $"A1: {input}";
}

[McpServerToolType]
public sealed class A2Tools
{
    [McpServerTool(Name = "A2"), Description("Echoes A2 calling.")]
    public static string A2(string input) => $"A2: {input}";
}