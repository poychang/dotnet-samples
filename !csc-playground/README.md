# 使用 csc.exe 編譯 C# 程式

csc 命令是指 Microsoft C# 編譯器，它是 Microsoft 提供的命令列工具，用於將 C# 原始程式碼編譯為可執行程式或庫。C# 編譯器通常稱為 csc.exe，是 .NET Framework 和 .NET Core 開發平臺的基本元件。

## 使用方法

將 C# 原始程式碼檔 （.cs） 編譯為可執行程式或庫。它讀取原始程式碼，執行語法和語義分析，生成中間語言 （IL） 代碼，並生成可由其他程式執行或引用的可執行檔（程式集）或庫 （DLL）。

```bash
csc.exe /target:exe hello-world.cs
```
`/target` 指定輸出檔案的格式，可設定成 `exe`、`winexe`、`library`、`module`

更多關於 csc.exe 的使用方法，可使用 `csc /?` 指令，查看文件。

## 參考資料

- [MS Learn - Csc task](https://learn.microsoft.com/zh-tw/visualstudio/msbuild/csc-task)
- [MS Learn - C# compiler options](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/)
