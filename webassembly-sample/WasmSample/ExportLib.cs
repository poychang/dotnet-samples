using System.Runtime.InteropServices.JavaScript;

namespace WasmLib
{
    public static partial class ExportLib
    {
        // JS 可直接呼叫：exports.MyWasm.Exports.Add(1, 2)
        [JSExport]
        public static int Add(int a, int b) => a + b;

        // 由 C# 反呼 JS（這裡直接綁 globalThis.console.log）
        [JSImport("console.log", "globalThis")]
        internal static partial void Log([JSMarshalAs<JSType.String>] string value);

        [JSExport]
        public static void Hello(string name) => Log($"Hi, {name}!");

        // 傳陣列（byte[] / int[] 都支援自動封送）
        [JSExport]
        public static byte[] DoubleBytes(byte[] data)
        {
            var result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) result[i] = (byte)(data[i] * 2);
            return result;
        }
    }
}
