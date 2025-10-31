// ANSI SGR
Console.WriteLine("=== 16 色（一般/亮色） ===");
Print16Colors();

// 產生 SGR 轉義序列：\x1b[<codes>m
static string Sgr(params int[] codes) => "\x1b[" + string.Join(';', codes) + "m";
static void Print16Colors()
{
    string[] names = ["Black ", "Red   ", "Green ", "Yellow", "Blue  ", "Magenta", "Cyan  ", "White "];

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
        Console.Write(" " + names[i] + " ");
        Console.Write(Sgr(0));
        Console.Write(" ");
    }
    Console.WriteLine();

    Console.WriteLine("亮背景 (100–107):");
    for (int i = 0; i < 8; i++)
    {
        Console.Write(Sgr(30 + (i == 7 ? 0 : 7)));
        Console.Write(Sgr(100 + i));
        Console.Write(" " + names[i] + " ");
        Console.Write(Sgr(0));
        Console.Write(" ");
    }
    Console.WriteLine();
    Console.WriteLine();
}