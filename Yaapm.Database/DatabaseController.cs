using System.Diagnostics;
using Database.Data;
using Database.Parser;

namespace Database;

public static class DatabaseController
{
    public static string RunProcessGetStdout(string command)
    {
        using var process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = "-c \"" + command + "\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        process.WaitForExit();
        return process.StandardOutput.ReadToEnd();
    }
    
    /// <summary>
    ///  
    /// </summary>
    /// <param name="name">Package name</param>
    /// <param name="version">Package version</param>
    /// <returns>Whether succeeded</returns>
    public static bool TryGetPackageVersion(string name, out string? version)
    {
        if (name.Trim() == string.Empty)
        {
            version = null;
            return false;
        }
        
        var stdout = RunProcessGetStdout("pacman -Q " + name);
        if (stdout.Contains("error:"))
        {
            version = null;
            return false;
        }
        
        try
        {
            version = stdout.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
        }
        catch (Exception)
        {
            version = null;
        }
        return version != null;
    }

    public static bool TryGetPkgInfo(string name, out PkgInfo? info)
    {
        var stdout = RunProcessGetStdout("pacman -Qi " + name);
        info = PkgInfoParser.Parse(stdout);
        return info != null;
    }
}