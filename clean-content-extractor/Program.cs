using Microsoft.Playwright;
using ReverseMarkdown;

class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("請輸入網址，例如：dotnet run -- https://example.com");
            return;
        }

        string url = args[0];

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // 注入 Readability.js
        var readabilityScript = await File.ReadAllTextAsync("Readability.js");
        await page.AddScriptTagAsync(new() { Content = readabilityScript });

        // 執行 Readability 並回傳主要內容
        var result = await page.EvaluateAsync(@"() => {
            const reader = new Readability(document, {
                debug: false,
                maxElemsToParse: 0,
                keepClasses: false,
            });
            const article = reader.parse();
            return article ? {
                title: article.title,
                byline: article.byline,
                content: article.content,
                length: article.length,
                excerpt: article.excerpt,
                siteName: article.siteName,
            } : null;
        }");
        /* 以下是 Readability.js 支援的參數（都為可選）：
         * | 參數名稱                | 預設值      | 說明                                       |
         * | ------------------- | -------- | ---------------------------------------- |
         * | `debug`             | `false`  | 是否輸出除錯訊息到 console                        |
         * | `maxElemsToParse`   | `0` (不限) | 限制要分析的 DOM 元素數量，避免解析大型頁面時過久              |
         * | `nbTopCandidates`   | `5`      | 要保留多少個最佳候選區塊做分析（通常不需要改）                  |
         * | `charThreshold`     | `500`    | 內文至少要有幾個字元才被認定為「主文」                      |
         * | `classesToPreserve` | `[]`     | 要保留的 class 名稱清單（例如 `["highlight"]`）      |
         * | `keepClasses`       | `false`  | 是否保留 HTML class 屬性（若為 `true`，不會刪掉 class） |
         */

        /* Readability.js 的 article = reader.parse() 會回傳一個物件：
         * | 屬性          | 型別              | 說明                 |
         * | ------------- | ----------------- | -------------------- |
         * | `title`       | `string`          | 標題                 |
         * | `byline`      | `string`          | 作者（若有）         |
         * | `dir`         | `"ltr"` / `"rtl"` | 文字方向             |
         * | `content`     | `string` (HTML)   | 乾淨的主文 HTML      |
         * | `textContent` | `string`          | 去除標籤的文字       |
         * | `length`      | `number`          | 字數統計             |
         * | `excerpt`     | `string`          | 摘要                 |
         * | `siteName`    | `string`          | 網站名稱（若抓得到） |
         */

        if (result is null)
        {
            Console.WriteLine("❌ 無法擷取主體內容。");
            return;
        }

        // 修正 'JsonElement?' 的問題，需先檢查是否為 null，並使用 Value 來存取 JsonElement
        var title = result.Value.GetProperty("title").GetString();
        var byline = result.Value.GetProperty("byline").GetString();
        var html = result.Value.GetProperty("content").GetString() ?? "";
        var length = result.Value.GetProperty("length").GetInt32();
        var excerpt = result.Value.GetProperty("excerpt").GetString();
        var siteName = result.Value.GetProperty("siteName").GetString();

        // 使用 ReverseMarkdown 轉換 HTML 為 Markdown
        var converter = new Converter();
        var markdown = converter.Convert(html);

        Console.WriteLine("\n====== 擷取結果 (Markdown) ======\n");
        Console.WriteLine($"# {title}");
        
        if (!string.IsNullOrWhiteSpace(byline)) Console.WriteLine($"_By: {byline}_\n");
        if (length > 0) Console.WriteLine($"_Count: {length}_\n");
        if (!string.IsNullOrWhiteSpace(excerpt)) Console.WriteLine($"_Excerpt: {excerpt}_\n");
        if (!string.IsNullOrWhiteSpace(siteName)) Console.WriteLine($"_SiteName: {siteName}_\n");

        Console.WriteLine(markdown);
    }
}
