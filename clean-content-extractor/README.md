# CleanContentExtractor

以下是結合 `Microsoft.Playwright` 和 `Readability.js`，實作 Firecrawl 類似功能：輸入網址，自動取得**乾淨的主體內文**。

---

## ✅ 專案名稱：`CleanContentExtractor`

### 📁 專案結構

```
CleanContentExtractor/
├── Program.cs
├── Readability.js   ← 從 Mozilla 下載的原始檔
├── CleanContentExtractor.csproj
```

---

## 1️⃣ 建立專案

```bash
dotnet new console -n CleanContentExtractor
cd CleanContentExtractor
dotnet add package Microsoft.Playwright
playwright install
```

---

## 2️⃣ 下載 Readability.js

從 Mozilla 官方 repo 複製這份 JS 檔案：

📥 下載連結：
[https://raw.githubusercontent.com/mozilla/readability/main/Readability.js](https://raw.githubusercontent.com/mozilla/readability/main/Readability.js)

另需 `Readability.js` 的依賴 DOM Parser，建議使用已渲染好的瀏覽器環境（Playwright 預設提供），所以 **不需額外安裝 JSDOM**。

---

## 3️⃣ Program.cs 內容

```csharp
using Microsoft.Playwright;

class Program
{
    public static async Task Main(string[] args)
    {
        string url = args.Length > 0 ? args[0] : "https://example.com";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(url);

        // 注入 Readability.js 腳本
        string readabilityScript = await File.ReadAllTextAsync("Readability.js");
        await page.AddScriptTagAsync(new() { Content = readabilityScript });

        // 執行 Readability 提取文章
        var article = await page.EvaluateAsync<string>(@"
            () => {
                const reader = new Readability(document);
                const article = reader.parse();
                return article ? article.textContent : '';
            }
        ");

        Console.WriteLine("====== 擷取結果 ======\n");
        Console.WriteLine(article);
    }
}
```

---

## 4️⃣ 執行方式

```bash
dotnet run -- "https://ithome.com.tw/news/169663"
```

---

## ✅ 輸出結果（會看到乾淨內文）

```
====== 擷取結果 ======

習慣用AI寫文章會讓腦袋變笨？
一項新研究指出，當人們使用 ChatGPT 輔助寫作時...
```

---

## 🔄 可擴充功能（進階）

* 儲存為純文字 / Markdown / JSON
* 擷取 `<title>`、`byline`、`content` 等欄位
* 多頁爬蟲（遞迴抓取內部連結）
* GUI 介面或 Web API 包裝

---

