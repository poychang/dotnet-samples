using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var endpoint = config["AzureOpenAI:Endpoint"] ?? string.Empty;
var apiKey = config["AzureOpenAI:Key"] ?? string.Empty;
var deploymentName = "gpt-4o-mini";

// 建立 kernel，並設定使用 Azure OpenAI 的 chat completion 功能
var kernelBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
var kernel = kernelBuilder.Build();

// ========================================
// 進行多輪對話，包含訊息歷史，讓模型能夠更好地理解上下文和對話角色，從而生成上下文連貫性的回應
// ========================================
Console.WriteLine("# ChatHistory ------------------------------");
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

var systemMessage = "Be kind, be grateful";
Console.WriteLine($"{AuthorRole.System}: {systemMessage}");
chatHistory.AddSystemMessage(systemMessage);

var userMessage = "hi, how are you";
chatHistory.AddUserMessage(userMessage);
Console.WriteLine($"{AuthorRole.User}: {userMessage}");

var response = await chatService.GetChatMessageContentAsync(chatHistory);
chatHistory.AddAssistantMessage(response.ToString());
Console.WriteLine($"{response.Role}: {response}");

Console.WriteLine();
Console.WriteLine("# ChatHistory JSON ------------------------------");
Console.WriteLine(JsonSerializer.Serialize(chatHistory, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine();
