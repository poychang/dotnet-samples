using System.Diagnostics;

public class DotnetCLI
{
    public static void RunDotnetCLIVersion()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"dotnet CLI version: {output.Trim()}");
            }
            else
            {
                Console.WriteLine("dotnet CLI occurs error:");
                Console.WriteLine(error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Dotnet CLI not found. Please ensure it is installed correctly.");
            Console.WriteLine($"Error message: {ex.Message}");
        }
    }
}
