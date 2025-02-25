using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
var endpoint = config["AzureOpenAI:Endpoint"] ?? string.Empty;
var apiKey = config["AzureOpenAI:Key"] ?? string.Empty;
var deploymentName = "gpt-4o-mini";

// Create a kernel with OpenAI chat completion
var kernelBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
var kernel = kernelBuilder.Build();

// Invoke the kernel with a prompt and display the result
Console.WriteLine("# Response ------------------------------");
var response = await kernel.InvokePromptAsync("What color is the sky?");
Console.WriteLine(response.ToString());
Console.WriteLine();

// Invoke the kernel with a prompt and stream the results to the display
Console.WriteLine("# Streaming ------------------------------");
await foreach (var update in kernel.InvokePromptStreamingAsync("What color is the sky?"))
{
    Console.Write(update.ToString());
}
