using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var endpoint = config["AzureOpenAI:Endpoint"] ?? string.Empty;
var apiKey = config["AzureOpenAI:APIKey"] ?? string.Empty;
var deploymentName = "gpt-4o-mini";

// 建立 kernel，並設定使用 Azure OpenAI 的 chat completion 功能
var kernelBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
var kernel = kernelBuilder.Build();

// ========================================
// 使用 prompt 來呼叫 kernel，並顯示結果
// ========================================
Console.WriteLine("# Response ------------------------------");
var response = await kernel.InvokePromptAsync("What color is the sky?");
Console.WriteLine(response);
Console.WriteLine();

// ========================================
// 使用 streaming 的方式來呼叫 kernel，並顯示結果
// ========================================
Console.WriteLine("# Streaming ------------------------------");
await foreach (var update in kernel.InvokePromptStreamingAsync("What color is the sky?"))
{
    Console.Write(update);
}
