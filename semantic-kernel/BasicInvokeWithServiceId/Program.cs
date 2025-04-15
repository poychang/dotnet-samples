using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var endpoint = config["AzureOpenAI:Endpoint"] ?? string.Empty;
var apiKey = config["AzureOpenAI:APIKey"] ?? string.Empty;
var deploymentName = "gpt-4o-mini";

// 建立 kernel，並設定使用 Azure OpenAI 的 chat completion 功能
// 這裡建立了兩個 serviceId，分別用於聊天和文字補全。同理，您也可以建立更多的 serviceId 來使用不同的模型，並在呼叫的時候指定要使用的 serviceId。
var kernelBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey, serviceId: "chat-model")
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey, serviceId: "completion-model");
var kernel = kernelBuilder.Build();

// ========================================
// 使用特定 serviceId 的模型來執行聊天
// ========================================
/// <see cref="IChatCompletionService"/> 專門針對對話式 AI 模型設計，會採用對話訊息的格式，包含多輪訊息歷史，讓模型能夠更好地理解上下文和對話角色，從而生成上下文連貫性的回應。
Console.WriteLine("# ServiceID: chat-model ------------------------------");
var chatService = kernel.GetRequiredService<IChatCompletionService>("chat-model");
var chatHistory = new ChatHistory("請介紹 Semantic Kernel");
var response = await chatService.GetChatMessageContentAsync(chatHistory);
Console.WriteLine(response);
Console.WriteLine();

// ========================================
// 使用特定 serviceId 的模型來執行文字補全
// ========================================
/// <see cref="ITextGenerationService"/> 主要用於一般的文字生成任務，通常是將一段純文字的 prompt 傳給模型，讓它續寫或補全文字。它適合用於非對話式的場景，像是文章撰寫、補全文字內容等。
Console.WriteLine("# ServiceID: completion-model ------------------------------");
var userInput = "請介紹 Semantic Kernel，這是關";
var prompt = $"請以以下內容為開頭並續寫出完整的句子：\n{userInput}\n";
var textGenerationService = kernel.GetRequiredService<ITextGenerationService>("completion-model");
var response2 = await textGenerationService.GetTextContentAsync(prompt);
Console.WriteLine(response2);
Console.WriteLine();

/// 順帶一提，除了使用內建的 Service 外，也可以用最基本的 <see cref="InvokePromptAsync"/> 搭配 <see cref="PromptExecutionSettings"/> 來呼叫指定 serviceId 的模型。
/// 不過此方法可能會遇到實驗性功能的警告，請注意。
/// 關於實驗性功能的警告，請參考 https://github.com/microsoft/semantic-kernel/blob/main/dotnet/docs/EXPERIMENTS.md 文件說明。
Console.WriteLine("# ServiceID: completion-model ------------------------------");
#pragma warning disable SKEXP0001
var result = await kernel.InvokePromptAsync(prompt, new(new PromptExecutionSettings { ServiceId = "completion-model" }));
#pragma warning restore SKEXP0001
Console.WriteLine(result);
