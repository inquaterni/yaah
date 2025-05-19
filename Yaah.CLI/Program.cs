using System.Runtime.InteropServices;
using CommandLine;
using NLog;
using NLog.Config;
using QuikGraph;
using QuikGraph.Algorithms;
using Yaapm.DReS;
using Yaapm.DReS.Optional;
using Yaapm.Net.InfoGathering;
using Yaapm.Net.Rpc;
using Yaapm.Net.Structs;
using Yaapm.System.Database;
using Yaapm.System.Process;

namespace Yaapm.CLI;

internal class Options
{
    [Option('S', "sync", HelpText = "Install or update the specified packages")]
    public bool Sync { get; set; }
    
    [Option('s', "search", HelpText = "Search for specified packages")]
    public bool Search { get; set; }
    
    [Option('D', "debug", HelpText = "Enable debug output")]
    public bool Debug { get; set; }
    
    [Option('g', "graph-serialization", HelpText = "Enable graph serialization")]
    public bool GraphSerialization { get; set; }
    
    [Value(0, MetaName = "parameters", Required = true, HelpText = "Packages to operate on", Min = 1)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public IEnumerable<string> Parameters { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

internal static partial class Program
{
    private static readonly RpcEngine Engine = new();
    private static readonly DatabaseController Db = new();
    private static readonly string CachePath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "~/", ".cache/yaah/");

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly LoggingConfiguration Config = new();

    [LibraryImport("libc")]
    private static partial uint getuid();
    
    private static T? Input<T>(string prompt, T? defaultValue = default, Func<string?, T>? filterFunc = null)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        var result = defaultValue;
        try
        {
            result = filterFunc != null ? filterFunc(input) : (T?)Convert.ChangeType(input, typeof(T));
        }
        catch (Exception e) when (e is FormatException or OverflowException or InvalidCastException)
        {
            Logger.Warn($"Conversion failed {e.GetType().Name}: {e.Message}");
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        
        return result;
    }

    private static void Main(string[] args)
    {
        if (getuid() == 0)
        {
            Console.WriteLine("Avoid running yaah as root/sudo.");
        }
        
        // var logFile = new NLog.Targets.FileTarget("logfile") {FileName = "log.txt"};
        var target = new NLog.Targets.ColoredConsoleTarget("console")
        {
            Layout = "[${longdate}|${level:uppercase=true}] ${logger}: ${message}"
        };
        Config.AddTarget(target);
        Config.AddRule(LogLevel.Info, LogLevel.Fatal, target);
        
        LogManager.Configuration = Config;
        LogManager.ReconfigExistingLoggers();
        
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(opts =>
            {
                if (opts.Debug)
                {
                    Config.AddRule(LogLevel.Debug, LogLevel.Fatal, target);
                    LogManager.ReconfigExistingLoggers();
                }
                
                Logger.Debug($"Parameters: {string.Join(", ", opts.Parameters.AsParallel().Select(x => "'" + x + "'"))}");
                
                if (opts.Sync)
                {
                    if (opts.Search)
                    {
                        if (opts.Parameters.ToArray().Length == 1)
                        {
                            Search(opts.Parameters.First());
                        }
                        else
                        {
                            Console.WriteLine("Search requires only one package");
                        }
                    }
                    else
                    {
                        Install(opts.Parameters.ToArray());
                    }
                }
                
                if (!opts.GraphSerialization || opts.Sync || opts.Search) return;
                if (opts.Parameters.ToArray().Length == 2)
                {
                    SerializeGraph(opts.Parameters.First(), opts.Parameters.Skip(1).First());
                }
            })
            .WithNotParsed(errs =>
            {
                Logger.Fatal(string.Join("\n", errs));
                Environment.Exit(-1);
            });
    }

    private static void Search(string package)
    {
        var searchRes = Engine.Search(package).Result;
        if (searchRes == null || searchRes.ResultCount == 0)
        {
            Console.WriteLine($"No packages found for query '{package}'");
            return;
        }

        var i = 0;
        foreach (var result in searchRes.Results)
        {
            var resultStr = $"{++i} {result.Name} {result.Version} ";
            if (result.OutOfDate != null)
            {
                var dto = DateTimeOffset.FromUnixTimeSeconds(result.OutOfDate.Value);
                var localDateTime = dto.LocalDateTime;
                resultStr += $"(Out of date: {localDateTime:yyyy-MM-dd})";
            }
            
            resultStr += $"\n\t{result.Description}";
            Console.WriteLine(resultStr);
        }
    }

    private static void Install(string[] packages)
    {
        var flags = "-si";
        List<BasicPkgInfo> pkgExplicit = [];
        
        Parallel.ForEachAsync(packages, async (pkg, token) =>
        {
            var pkgs = await Engine.Search(pkg, token: token);
            if (pkgs is null || pkgs.ResultCount == 0 || !pkgs.Results.AsParallel().Any(x => x.Name.Equals(pkg)))
            {
                Logger.Debug($"Package '{pkg}' not found");
                return;
            }
            pkgExplicit.Add(pkgs.Results.AsParallel().Where(x => x.Name.Equals(pkg)).First());
        }).Wait();

        if (pkgExplicit.Count == 0)
        {
            Logger.Fatal("No packages found");
            return;
        }
        
        Console.WriteLine($"AUR Explicit({pkgExplicit.Count}): {string.Join(", ", pkgExplicit.AsParallel().Select(x => $"{x.Name}-{x.Version}"))}");
        
        flags += Input("Clean build? [Y/n]\n", "c", input =>
        {
            if (input != null && input.Equals("n", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            return "c";
        });
        Logger.Debug($"Flags updated: {flags}");

        flags += Input("Remove make dependencies after installation? [y/N]\n", string.Empty, input =>
        {
            if (input != null && input.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                return "r";
            }
            return string.Empty;
        });
        Logger.Debug($"Flags updated: {flags}");

        var inspector = new PackageInspector(Db.GetLocalDb());
        var table = inspector.GatherPackageInfo(pkgExplicit.Select(x => x.Name)).Result;

        var graph = Graph.BuildFor(pkgExplicit.AsParallel().Select(x => x.Name), table);

        if (!graph.IsDirectedAcyclicGraph())
        {
            Logger.Debug("Graph contains cycles, exiting...");
            Console.WriteLine("Cannot resolve dependencies, manual intervention is required");
            Environment.Exit(4);
        }

        if (!Directory.Exists(CachePath))
        {
            Logger.Debug($"Creating cache directory: '{CachePath}'");
            Directory.CreateDirectory(CachePath);
        }
        Directory.SetCurrentDirectory(CachePath);

        var installOrder = Graph.GetInstallOrder(graph).ToArray();
        Logger.Debug($"Install order: {string.Join(", ", installOrder)}");
        
        Parallel.ForEach(installOrder, pkg =>
        {
            var detailedPkgInfo = table[pkg] as DetailedPkgInfo;

            ShellRunner.Run(Directory.Exists(pkg)
                ? $"git -C {pkg} pull"
                : $"git clone https://aur.archlinux.org/{detailedPkgInfo!.Name}.git");
        });
        var i = 0;
        foreach (var pkg in installOrder)
        {
            if (!Directory.Exists(pkg))
            {
                Logger.Fatal($"Package directory {Path.Combine(CachePath, pkg)} does not exist");
                Console.WriteLine($"ERROR: Could not find installation directory for pkg '{pkg}'");
                Environment.Exit(-1);
            }
            
            ShellRunner.Run($"makepkg -D {pkg} " + flags + (i++ != 0 ? " --skippgpcheck" : ""));
        }
    }

    private static void SerializeGraph(string package, string path)
    {
        Logger.Debug($"Serializing graph to '{path}'");
        var dir = path[..(path.LastIndexOf('/') + 1)];
        if (!Directory.Exists(dir))
        {
            Logger.Fatal("Directory not found, exiting...");
            Console.WriteLine($"Directory not found: '{dir}'");
            return;
        }

        var inspector = new PackageInspector(Db.GetLocalDb());
        var table = inspector.GatherPackageInfo(package).Result;

        var graph = Graph.BuildFor(package, table);
        var algorithm = new QuikGraph.Graphviz.GraphvizAlgorithm<string, Edge<string>>(graph);
        algorithm.FormatVertex += (_, args) =>
        {
            args.VertexFormat.Label = args.Vertex;
        };
        algorithm.Generate(new FileDotEngine(), path);
    }
}