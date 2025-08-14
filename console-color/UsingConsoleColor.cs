public class UsingConsoleColor
{
    public static void Demo()
    {
        // 使用 ConsoleColor 類別來設定前景色和背景色
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Hello, World!");
        Console.ResetColor(); // 重設顏色

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Hello, World!");
        Console.ResetColor(); // 重設顏色
    }
 }