using Yaapm.RPC;

namespace Yaapm.CLI;

internal static class Program
{
    private static void Main(string[] args)
    {
        var rpcEngine = new RpcEngine();
        
        Console.WriteLine(string.Join(", ", rpcEngine.Suggest("zen-browser").Result ?? ["N/A"]));
    }
}