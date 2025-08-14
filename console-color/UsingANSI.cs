using System.Runtime.InteropServices;
using System.Text;

/*
# ANSI SGR

ANSI SGR（Select Graphic Rendition）是一種用於控制文字樣式的 ANSI escape code，常見於終端機（terminal）或命令列介面中。
它可以改變文字的顏色、背景色、粗體、底線等格式。

## ANSI SGR 格式基本結構：

```plaintext
ESC [ <SGR參數> m
```

- `ESC` 是 escape 字元（在程式中通常是 `\x1b` 或 `\033`）
- `[` 是引導字元
- `<SGR參數>` 是一組數字，代表不同的樣式設定
- `m` 是結尾字元，表示這是一個 SGR 指令

## 常見的 SGR 參數：

| 數值    | 效果                     |
| ------- | ------------------------ |
| 0       | 重設所有樣式             |
| 1       | 粗體                     |
| 2       | 微亮                     |
| 3       | 斜體（不是所有終端支援） |
| 4       | 底線                     |
| 7       | 反白                     |
| 22      | 取消粗體                 |
| 24      | 取消底線                 |
| 30–37   | 前景色（文字顏色）       |
| 39      | 重設前景色               |
| 40–47   | 背景色                   |
| 49      | 重設背景色               |
| 90–97   | 高亮前景色               |
| 100–107 | 高亮背景色               |

## 顏色範例

- `\x1b[31m`：紅色文字
- `\x1b[42m`：綠色背景
- `\x1b[1;34m`：粗體藍色文字
- `\x1b[0m`：重設樣式（通常在文字結尾加上）

## 範例程式碼

```csharp
Console.WriteLine("\x1b[1;31m這是紅色粗體文字\x1b[0m");
```

這段程式會在支援 ANSI 的終端機中顯示紅色粗體文字，然後重設樣式。

*/
public class UsingANSI
{

    public static void Demo()
    {
        // 是否生效取決於你的終端是否支援 ANSI（Windows Terminal／新式 PowerShell/VS Code 終端沒問題；傳統 cmd 可能需啟用 VT）
        var data = "Hello, World!";
        Console.WriteLine($"\x1b[1;33m{data}\x1b[0m");
        Console.WriteLine("Hello, World!");
    }
    
    // 產生 SGR 轉義序列：\x1b[<codes>m
    static string Sgr(params int[] codes) => "\x1b[" + string.Join(';', codes) + "m";

    public static void RunColorTable()
    {
        // 讓 Unicode 方塊/空白顯示正常（可有可無）
        Console.OutputEncoding = Encoding.UTF8;

        EnableVirtualTerminalOnWindows();

        Console.WriteLine("=== 16 色（一般/亮色） ===");
        Print16Colors();

        Console.WriteLine();
        Console.WriteLine("=== 256 色（色立方 16..231） ===");
        Print256Cube();

        Console.WriteLine();
        Console.WriteLine("=== 256 色（灰階 232..255） ===");
        Print256Gray();

        Console.WriteLine();
        Console.WriteLine("=== TrueColor 24-bit 彩虹漸層 ===");
        PrintTrueColorGradient();

        // 重設樣式
        Console.Write(Sgr(0));
        Console.WriteLine();
    }

    // ====== 16 色 ======
    static void Print16Colors()
    {
        string[] names = { "Black ", "Red   ", "Green ", "Yellow", "Blue  ", "Magenta", "Cyan  ", "White " };

        Console.WriteLine("前景 (30–37):");
        for (int i = 0; i < 8; i++)
        {
            Console.Write(Sgr(30 + i));
            Console.Write(names[i] + " ");
        }
        Console.Write(Sgr(0));
        Console.WriteLine();

        Console.WriteLine("亮前景 (90–97):");
        for (int i = 0; i < 8; i++)
        {
            Console.Write(Sgr(90 + i));
            Console.Write(names[i] + " ");
        }
        Console.Write(Sgr(0));
        Console.WriteLine();

        Console.WriteLine("背景 (40–47):");
        for (int i = 0; i < 8; i++)
        {
            Console.Write(Sgr(30 + (i == 0 ? 7 : 0))); // 讓字在背景上看得到：黑底白字/白底黑字
            Console.Write(Sgr(40 + i));
            Console.Write("  " + names[i] + "  ");
            Console.Write(Sgr(0));
            Console.Write(" ");
        }
        Console.WriteLine();

        Console.WriteLine("亮背景 (100–107):");
        for (int i = 0; i < 8; i++)
        {
            Console.Write(Sgr(30 + (i == 7 ? 0 : 7)));
            Console.Write(Sgr(100 + i));
            Console.Write("  " + names[i] + "  ");
            Console.Write(Sgr(0));
            Console.Write(" ");
        }
        Console.WriteLine();
    }

    // ====== 256 色：色立方（16..231） ======
    static void Print256Cube()
    {
        // 6×6×6：索引 = 16 + 36*r + 6*g + b
        for (int g = 0; g < 6; g++)
        {
            for (int r = 0; r < 6; r++)
            {
                for (int b = 0; b < 6; b++)
                {
                    int idx = 16 + 36 * r + 6 * g + b;
                    Console.Write(Sgr(48, 5, idx)); // 背景
                    Console.Write("  ");
                }
                Console.Write(Sgr(0));
                Console.Write(" "); // 組間空格
            }
            Console.WriteLine();
        }
    }

    // ====== 256 色：灰階（232..255） ======
    static void Print256Gray()
    {
        for (int idx = 232; idx <= 255; idx++)
        {
            Console.Write(Sgr(48, 5, idx));
            Console.Write("  ");
        }
        Console.Write(Sgr(0));
        Console.WriteLine();
    }

    // ====== TrueColor 24-bit 漸層 ======
    static void PrintTrueColorGradient()
    {
        int width = 80; // 一行 80 個色塊
        for (int i = 0; i < width; i++)
        {
            double h = i * 360.0 / width; // 0..360
            (int r, int g, int b) = HsvToRgb(h, 1.0, 1.0);
            Console.Write(Sgr(48, 2, r, g, b)); // 背景用 TrueColor
            Console.Write("  ");
        }
        Console.Write(Sgr(0));
        Console.WriteLine();
    }

    // HSV(0..360, 0..1, 0..1) -> RGB(0..255)
    static (int r, int g, int b) HsvToRgb(double h, double s, double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = v - c;
        double r1 = 0, g1 = 0, b1 = 0;

        if (h < 60) { r1 = c; g1 = x; b1 = 0; }
        else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
        else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
        else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
        else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
        else { r1 = c; g1 = 0; b1 = x; }

        int r = (int)Math.Round((r1 + m) * 255);
        int g = (int)Math.Round((g1 + m) * 255);
        int b = (int)Math.Round((b1 + m) * 255);
        return (r, g, b);
    }

    // 在舊版 Windows conhost 啟用 VT 支援（Windows Terminal / PowerShell 7 通常不需要）
    static void EnableVirtualTerminalOnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        const int STD_OUTPUT_HANDLE = -11;
        const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        IntPtr h = GetStdHandle(STD_OUTPUT_HANDLE);
        if (GetConsoleMode(h, out int mode))
        {
            SetConsoleMode(h, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
    }

    [DllImport("kernel32.dll")] static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll")] static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);
    [DllImport("kernel32.dll")] static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);
}