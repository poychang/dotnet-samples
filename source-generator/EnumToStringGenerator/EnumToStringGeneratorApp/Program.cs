using EnumToStringGenerator;
using System.ComponentModel;

namespace EnumToStringGeneratorApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var today = DateTime.Today;
            Console.WriteLine(DateTime.Today.DayOfWeek.FasterToString());
            Console.ReadLine();
        }
    }

    [Description("This is a test class for EnumToStringGenerator")]
    [EnumToString(typeof(DayOfWeek))]
    public static partial class DayOfWeekExtensions;
}
