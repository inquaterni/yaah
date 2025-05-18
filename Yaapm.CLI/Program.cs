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
using static Yaapm.System.FileSystem.FileSystemController;
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
    
    [Value(0, MetaName = "packages", Required = true, HelpText = "Packages to operate on", Min = 1)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public IEnumerable<string> Packages { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

internal static class Program
{
    private static readonly RpcEngine Engine = new();
    private static readonly DatabaseController Db = new();
    private static readonly string CachePath = Path.Combine(GetHomePath(), ".cache/yaah/");

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly LoggingConfiguration Config = new();

    private static void Main(string[] args)
    {
        
        // var logFile = new NLog.Targets.FileTarget("logfile") {FileName = "log.txt"};
        var logConsole = new NLog.Targets.ConsoleTarget("logconsole");
        
        // configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, logFile);
        // Config.AddRule(LogLevel.Trace, LogLevel.Fatal, logConsole);
        
        Config.Variables["logLevel"] = "Info";
        LogManager.Configuration = Config;
        LogManager.ReconfigExistingLoggers();
        
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(opts =>
            {
                if (opts.Sync)
                {
                    if (opts.Search)
                    {
                        if (opts.Packages.ToArray().Length == 1)
                        {
                            Search(opts.Packages.First());
                        }
                        else
                        {
                            Console.WriteLine("Search requires only one package");
                        }
                    }
                    else
                    {
                        Install(opts.Packages.ToArray());
                    }
                }

                if (!opts.Debug) return;
                
                Config.Variables["logLevel"] = "Debug";
                LogManager.Configuration = Config;
                LogManager.ReconfigExistingLoggers();
                
                if (!opts.GraphSerialization || opts.Sync || opts.Search) return;
                if (opts.Packages.ToArray().Length == 2)
                {
                    SerializeGraph(opts.Packages.First(), opts.Packages.Skip(1).First());
                }
            })
            .WithNotParsed(errs =>
            {
                Console.WriteLine(errs);
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
        List<string> pkgExplicit = [];
        var db = Db.GetLocalDb();
        
        Parallel.ForEachAsync(packages, async (pkg, token) =>
        {
            var pkgs = await Engine.Search(pkg, token: token);
            if (pkgs is null || pkgs.ResultCount == 0 || !pkgs.Results.Any(x => x.Name.Equals(pkg)))
            {
                Logger.Debug($"Package '{pkg}' not found");
                return;
            }
            pkgExplicit.Add(pkg);
        });
        
        Console.WriteLine($"AUR Explicit({pkgExplicit.Count}): {string.Join(", ", pkgExplicit)}");
        
        Console.WriteLine("Proceed with installation? [Y/n]");
        if (Console.Read().Equals('n'))
        {
            Environment.Exit(0);
        }
        Console.WriteLine("Clean build? [Y/n]");
        flags += Console.Read().ToString().Equals("n", StringComparison.CurrentCultureIgnoreCase) ? "" : "C";
        Console.WriteLine("Remove make dependencies after installation? [y/N]");
        flags += Console.Read().ToString().Equals("y", StringComparison.CurrentCultureIgnoreCase) ? "r" : "";

        var inspector = new PackageInspector(Db.GetLocalDb());
        var table = inspector.GatherPackageInfo(pkgExplicit).Result;

        var graph = Graph.BuildFor(pkgExplicit, table);

        if (!graph.IsDirectedAcyclicGraph())
        {
            Console.WriteLine("Cannot resolve dependencies, manual intervention is required");
            Environment.Exit(4);
        }
        
        if (!Directory.Exists(CachePath)) Directory.CreateDirectory(CachePath);
        Directory.SetCurrentDirectory(CachePath);

        var installOrder = Graph.GetInstallOrder(graph).ToArray();
        
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
                Console.WriteLine($"ERROR: Could not find installation directory for pkg '{pkg}'");
                Environment.Exit(-1);
            }
            
            ShellRunner.Run($"makepkg -D {pkg} " + flags + (i++ != 0 ? " --skippgpcheck" : ""));
        }
    }

    private static void SerializeGraph(string package, string path)
    {
        var dir = path[..(path.LastIndexOf('/') + 1)];
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Directory not found: '{dir}'");
        }

        var inspector = new PackageInspector(Db.GetLocalDb());
        var table = inspector.GatherPackageInfo(package).Result;

        var graph = Graph.BuildFor(package, table);

        if (!graph.IsDirectedAcyclicGraph())
        {
            Console.WriteLine("Cannot resolve dependencies, manual intervention is required");
            Environment.Exit(4);
        }
        
        var algorithm = new QuikGraph.Graphviz.GraphvizAlgorithm<string, Edge<string>>(graph);
        
        algorithm.FormatVertex += (_, args) =>
        {
            args.VertexFormat.Label = args.Vertex;
        };
        
        algorithm.Generate(new FileDotEngine(), path);
    }
}