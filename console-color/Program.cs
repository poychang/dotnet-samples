Console.WriteLine("------------------------------\n");
// 透過改變前景色來改變輸出文字的顏色
UsingConsoleColor.Demo();

Console.WriteLine("------------------------------\n");
// 使用 ANSI SGR (Select Graphic Rendition) 終端機控制碼來設定輸出的文字樣式
UsingANSI.Demo();
UsingANSI.RunColorTable();
