using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;

/// <summary>
/// This example uses all and only Azure services
/// https://github.com/microsoft/kernel-memory/blob/main/examples/007-dotnet-serverless-azure/README.md
/// </summary>
public class Program
{
    private static MemoryServerless? s_memory;

    private const string IndexName = "example006";

    public static async Task Main()
    {
        var azureAISearchConfig = new AzureAISearchConfig();
        var azureBlobConfig = new AzureBlobsConfig();
        var azureOpenAIEmbeddingConfig = new AzureOpenAIConfig();
        var azureOpenAITextConfig = new AzureOpenAIConfig();

        new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build()
            .BindSection("AzureAISearch", azureAISearchConfig)
            //.BindSection("AzureBlobs", azureBlobConfig)
            .BindSection("AzureOpenAIEmbedding", azureOpenAIEmbeddingConfig)
            .BindSection("AzureOpenAIText", azureOpenAITextConfig);

        var builder = new KernelMemoryBuilder()
            //.WithAzureBlobsDocumentStorage(azureBlobConfig)
            .WithAzureOpenAITextEmbeddingGeneration(azureOpenAIEmbeddingConfig)
            .WithAzureOpenAITextGeneration(azureOpenAITextConfig)
            .WithAzureAISearchMemoryDb(azureAISearchConfig);

        s_memory = builder.Build<MemoryServerless>(new KernelMemoryBuilderBuildOptions
        {
            AllowMixingVolatileAndPersistentData = true,
        });

        // ====== Store some data ======
        await StoreWebPageAsync(); // Works with Azure AI Search and Azure OpenAI

        // ====== Answer some questions ======
        // Test 1 (answer from the web page)
        var question = "What's Kernel Memory?";
        Console.WriteLine($"Question: {question}");
        var answer = await s_memory.AskAsync(question, index: IndexName);
        Console.WriteLine($"Answer: {answer.Result}\n\n");

        // Test 2 (requires Azure AI Document Intelligence to have parsed the image)
        question = "Which conference is Microsoft sponsoring?";
        Console.WriteLine($"Question: {question}");
        answer = await s_memory.AskAsync(question, index: IndexName);
        Console.WriteLine($"Answer: {answer.Result}\n\n");
    }

    // Downloading web pages
    private static async Task StoreWebPageAsync()
    {
        const string DocId = "webPage1";
        if (!await s_memory!.IsDocumentReadyAsync(DocId, index: IndexName))
        {
            Console.WriteLine("Uploading https://raw.githubusercontent.com/microsoft/kernel-memory/main/README.md");
            await s_memory.ImportWebPageAsync("https://raw.githubusercontent.com/microsoft/kernel-memory/main/README.md", index: IndexName, documentId: DocId);
        }
        else
        {
            Console.WriteLine($"{DocId} already uploaded.");
        }
    }
}