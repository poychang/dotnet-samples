using System.Diagnostics;
using System.Xml.Linq;

public class PowerShell
{
    public static void ExecuteFile()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                ArgumentList = {
                        "-NoProfile",
                        "-ExecutionPolicy", "Bypass",
                        "-File", Path.Combine(AppContext.BaseDirectory, "PowerShell", "Scripts", "hello-world.ps1"),
                    },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = Process.Start(processStartInfo) ?? throw new Exception("Failed to start pwsh");
            process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine($"\x1b[0m{e.Data}"); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine($"\x1b[31m{e.Data}"); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0) Environment.Exit(process.ExitCode);

            Console.WriteLine("Done.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error message: {ex.Message}");
        }
    }

    public static void ExecuteFileWithParam()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                ArgumentList = {
                        "-NoProfile",
                        "-ExecutionPolicy", "Bypass",
                        "-File", Path.Combine(AppContext.BaseDirectory, "PowerShell", "Scripts", "with-param.ps1"),
                        "-Path", Path.Combine(AppContext.BaseDirectory, "PowerShell", "Tools", "ffmpeg.exe"),
                    },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = Process.Start(processStartInfo) ?? throw new Exception("Failed to start pwsh");
            process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine($"\x1b[31m{e.Data}\x1b[0m"); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0) Environment.Exit(process.ExitCode);

            Console.WriteLine("Done.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error message: {ex.Message}");
        }
    }
}
