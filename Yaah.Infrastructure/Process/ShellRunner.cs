using System.Diagnostics;

namespace Yaah.Infrastructure.Process;

public static class ShellRunner
{
    private static readonly string Shell = Environment.GetEnvironmentVariable("SHELL") ?? "/usr/bin/bash";

    public static void Run(string cmd)
    {
        if (Shell == null) throw new ArgumentException("Environment variable SHELL not set");

        var startInfo = new ProcessStartInfo(Shell)
        {
            UseShellExecute = true,
            Arguments = $"-c \"{cmd}\""
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null) throw new ArgumentException("Failed to run process");

        process.WaitForExit();
    }
}