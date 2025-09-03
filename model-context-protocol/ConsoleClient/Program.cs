using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

// 讀取設定檔
var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

// 準備 Semantic Kernel
// ------------------------------------------------------------
var builder = Kernel.CreateBuilder();
// builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

// 設定 Azure OpenAI API
builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: config["AzureOpenAI:DeploymentName"] ?? "gpt-4o",
    endpoint: config["AzureOpenAI:Endpoint"] ?? "",
    apiKey: config["AzureOpenAI:APIKey"] ?? "",
    httpClient: HttpLogger.GetHttpClient(true)
);
var kernel = builder.Build();


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
var remoteTransport = new SseClientTransport(new() {
    Name = "RemoteMcpServer",
    Endpoint = new Uri("http://localhost:3001/mcp"),
});

// 注意：這裡假設 MCP Server 可在本地端運行，並且可以透過 StdioClientTransport 連接
await using IMcpClient mcpClient = await McpClientFactory.CreateAsync(remoteTransport);


// 取得 MCP Tools 清單
// ------------------------------------------------------------
var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
// 列出 MCP工具名稱和描述
Console.WriteLine("\nAvailable MCP Tools:");
foreach (var tool in tools)
{
    Console.WriteLine($"\t{tool.Name}: {tool.Description}");
}
Console.WriteLine();


// 將 MCP 工具轉換為 Semantic Kernel 函數並註冊到 Semantic Kernel 中
// ------------------------------------------------------------
kernel.Plugins.AddFromFunctions("McpTools", tools.Select(t => t.AsKernelFunction()));



// 測試具有 MCP 工具的對話
// ------------------------------------------------------------
// Create chat history 物件，並且加入系統訊息
var history = new ChatHistory();
history.AddSystemMessage("你是一位 Model Context Protocol 工具助理，會根據使用者輸入決定是否要使用 tool 來回答問題。");

// Get chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// 開始對談
Console.Write("\x1b[44mUser >\u001b[0m ");
string? userInput;
while (!string.IsNullOrEmpty(userInput = Console.ReadLine()))
{
    // Add user input
    history.AddUserMessage(userInput);

    // Enable auto function calling
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("\x1b[42mAssistant >\x1b[0m " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);

    // Get user input again
    Console.Write("\x1b[44mUser >\u001b[0m ");
}

Console.WriteLine("\n\nExiting...");
await mcpClient.DisposeAsync().ConfigureAwait(false);
