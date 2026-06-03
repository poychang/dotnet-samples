using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using System.ClientModel.Primitives;

// 讀取設定檔
var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();


// 建立 MCP client
// ------------------------------------------------------------
// 建立可在本地端運行的 MCP Server，並透過 StdioClientTransport 連接
var localProjectTransport = new StdioClientTransport(new()
{
    Name = "LocalMcpServer",
    // 假設 MCP Server 的執行檔在使用 dotnet CLI 所執行的指定專案
    Command = "dotnet",
    Arguments = ["run", "--project", "../LocalServer/LocalServer.csproj"],
});
var localExeTransport = new StdioClientTransport(new()
{
    Name = "LocalMcpServer",
    // 假設 MCP Server 的執行檔在 "c:\tools\mcp\LocalServer.exe" 路徑下
    Command = "C:\\tools\\mcp\\LocalServer.exe",
    Arguments = [],
});
// 建立在遠端運行的 MCP Server，並透過 SseClientTransport 連接
var remoteTransport = new HttpClientTransport(new()
{
    Name = "RemoteMcpServer",
    Endpoint = new Uri("http://localhost:3001/mcp"),
});

// 注意：這裡假設 MCP Server 可在本地端運行，並且可以透過 StdioClientTransport 連接
await using var mcpClient = await McpClient.CreateAsync(remoteTransport);


// 取得 MCP Tools 清單
// ------------------------------------------------------------
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
// 列出 MCP工具名稱和描述
Console.WriteLine("\nAvailable MCP Tools:");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"\t{tool.Name}: {tool.Description}");
}
Console.WriteLine();


// 準備 Foundry Agent
// ------------------------------------------------------------
var endpoint = new Uri(config["PC:MicrosoftFoundry:Endpoint"] ?? "https://RESOURCE-NAME.openai.azure.com/");
var credential = new ClientSecretCredential(
    config["PC:MicrosoftFoundry:TenantId"],
    config["PC:MicrosoftFoundry:ClientId"],
    config["PC:MicrosoftFoundry:ClientSecret"]);
var clientOptions = new AIProjectClientOptions();
clientOptions.AddPolicy(new HttpTrafficLoggingPolicy(), PipelinePosition.PerCall);

AIAgent agent = new AIProjectClient(endpoint, credential, clientOptions)
    .AsAIAgent(
        model: config["PC:MicrosoftFoundry:DeploymentName"] ?? "MODEL_NAME",
        instructions: "你是一位 Model Context Protocol 工具助理，會根據使用者輸入決定是否要使用 tool 來回答問題。",
        name: "MCPAgent",
        tools: [.. mcpTools.Cast<AITool>()]
    );


// 測試具有 MCP 工具的對話
// ------------------------------------------------------------

// 開始對談
Console.Write("\x1b[44mUser >\u001b[0m ");
string? userInput;
while (!string.IsNullOrEmpty(userInput = Console.ReadLine()))
{
    // Print the results
    Console.WriteLine("\x1b[42mAssistant >\x1b[0m " + await agent.RunAsync(userInput));

    // Get user input again
    Console.Write("\x1b[44mUser >\u001b[0m ");
}

Console.WriteLine("\n\nExiting...");
await mcpClient.DisposeAsync().ConfigureAwait(false);
