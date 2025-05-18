using System.Diagnostics;

namespace Yaapm.System.Process;

public class ShellRunner
{
    private static readonly string? Shell = Environment.GetEnvironmentVariable("SHELL");

    public static int Run(string cmd)
    {
        if (Shell == null) throw new ArgumentException("Environment variable SHELL not set");
        
        var startInfo = new ProcessStartInfo(Shell)
        {
            UseShellExecute = true,
            Arguments = $"-c \"{cmd}\""
        };

        using var process = global::System.Diagnostics.Process.Start(startInfo);
        if (process == null)
        {
            throw new ArgumentException("Failed to run process");
            return -1;
        }
        
        process.WaitForExit();
        return process.ExitCode;
    }
}