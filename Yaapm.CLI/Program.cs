using Database;

namespace Yaapm.CLI;

internal static class Program
{
    private static void Main(string[] args)
    {
        DatabaseController.TryGetPkgInfo("gtk3", out var info);
        Console.WriteLine(info?.InstallDate.GetType());
    }
}