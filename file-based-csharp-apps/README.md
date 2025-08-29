# File-Based C# Apps

使用 `dotnet run app.cs` 直接執行 C# 檔案，這意味著不再需要建立專案檔即可快速執行腳本、測試程式碼片段或嘗試想法。

它簡單、直觀，旨在簡化 C# 開發體驗，特別是對於剛入門的玩家。

> 此功能僅支援在 .NET 10 預覽版之後的版本執行。

## 使用 #:p ackage 參考 NuGet 套件

可以使用 `#:p ackage` 指示詞直接在 `.cs` 檔案中新增 NuGet 套件參考：

```csharp
#:package Humanizer@2.14.1

using Humanizer;

var dotNet9Released = DateTimeOffset.Parse("2025-08-30");
var since = DateTimeOffset.Now - dotNet9Released;

Console.WriteLine($"It has been {since.Humanize()} since .NET 9 was released.");
```

## 使用 #：sdk 指定 SDK

預設會使用 Microsoft.NET.Sdk SDK，如果需要建置類似 Web API 的程式，則可以使用 `#:sdk` 指示詞來變更 SDK：

```csharp
#:sdk Microsoft.NET.Sdk.Web
```

上面的設定會這將該檔案視為 Web 專案的一部分，從而啟用 ASP.NET Core 的相關功能，例如 Minimal API 和 MVC。

## 使用 #:property 設定 MSBuild 屬性

可以使用 `#:property` 來設定其他建置屬性。例如將建置的 C# 語言版本改成 `preview`：

```csharp
#:property LangVersion preview
```

這可讓檔案型應用程式選擇使用進階語言功能和平台目標，而不需要完整的 `.csproj` 專案檔。

## 將 shebang 用於 shell 腳本

檔案型應用程式支援 [shebang](https://en.wikipedia.org/wiki/Shebang_%28Unix%29)（`#！`），允許我們編寫可直接在類 Unix 系統上執行的跨平台 C# shell 腳本。

例如以下檔案會使用 `/usr/bin/dotnet` 程式對此檔案進行 `run` 執行動作：

```csharp
#!/usr/bin/dotnet run
Console.WriteLine("Hello from a C# script!");
```

在類 Unix 系統上，我們必須將該檔案設為可執行檔，才能直接執行：

```shell
chmod +x app.cs  # 設定成可執行檔
./app.cs         # 直接執行
```

藉此將此檔案型應用程式變成 CLI 程式、自動化腳本和工具，讓使用者方便直接執行。

## 轉換為專案型應用程式

當撰寫的檔案型應用程式變得複雜時，或者想要使用專案型應用程式所提供的額外功能，可以使用以下方式將該檔案轉成標準的專案結構：

```shell
dotnet project convert app.cs
```

此命令已檔案命來建立新的資料夾，在資料夾中會額外建立 `.csproj` 專案檔，原本程式碼中的 `#:` 指示詞會轉譯成 MSBuild 屬性和參考，並將程式碼移至 `Program.cs` 檔案中。

## Reference

- [No projects just C# with `dotnet run app.cs` | DEM518](https://www.youtube.com/watch?v=98MizuB7i-w)
- [Announcing dotnet run app.cs – A simpler way to start with C# and .NET 10](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
- [GitHub - dotnet/sdk](https://github.com/dotnet/sdk/blob/main/documentation/general/dotnet-run-file.md)
