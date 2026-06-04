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
        var domain = httpContextAccessor
            .HttpContext?
            .Request
            .RouteValues["domain"]?
            .ToString();
        var visibleTools = domain switch
        {
            "domain1" => new[] { McpServerTool.Create(Domain1Tools.A1), McpServerTool.Create(Domain1Tools.A2) },
            "domain2" => new[] { McpServerTool.Create(Domain2Tools.B1) },
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
        var domain = httpContextAccessor
            .HttpContext?
            .Request
            .RouteValues["domain"]?
            .ToString();
        var visibleTools = domain switch
        {
            "domain1" => new[] { McpServerTool.Create(Domain1Tools.A1), McpServerTool.Create(Domain1Tools.A2) },
            "domain2" => new[] { McpServerTool.Create(Domain2Tools.B1) },
            _ => []
        };
        var allowedToolNames = visibleTools.Select(p => p.ProtocolTool.Name);
        var requestedToolName = context.Params.Name;

        if (!allowedToolNames.Contains(requestedToolName))
        {
            throw new McpProtocolException(
                $"Tool '{requestedToolName}' is not available on endpoint '{domain}'.",
                McpErrorCode.InvalidRequest);
        }

        var tool = visibleTools.Single(t => t.ProtocolTool.Name == requestedToolName);

        return await tool.InvokeAsync(context, ct);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapMcp("/{domain}/mcp");

app.Run("https://localhost:3001");



[McpServerToolType]
public sealed class Domain1Tools
{
    [McpServerTool, Description("Echoes A1 calling.")]
    public static string A1(string input) => $"A1: {input}";

    [McpServerTool, Description("Echoes A2 calling.")]
    public static string A2(string input) => $"A2: {input}";
}

[McpServerToolType]
public sealed class Domain2Tools
{
    [McpServerTool, Description("Echoes B1 calling.")]
    public static string B1(string input) => $"B1: {input}";
}