using System.Collections;
using System.Runtime.InteropServices;
using CommandLine;
using NLog;
using NLog.Config;
using NLog.Targets;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Graphviz;
using Yaah.DReS;
using Yaah.Infrastructure.Database;
using Yaah.Infrastructure.Process;
using Yaah.Net.InfoGathering;
using Yaah.Net.Models;
using Yaah.Net.RPC;
using FileDotEngine = Yaah.DReS.Optional.FileDotEngine;

namespace Yaah.CLI;

internal static partial class Program
{
    private static readonly RpcEngine Engine = new();
    private static readonly DatabaseController Db = new();

    private static readonly string CachePath =
        Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "~/", ".cache/yaah/");

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        var target = ConfigureLogger();
        UnhandledExceptionHook();

        if (getuid() == 0) Console.WriteLine("Avoid running yaah as root/sudo.");

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(opts =>
            {
                if (opts.Debug)
                {
                    LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, target);
                    LogManager.ReconfigExistingLoggers();
                }

                if (opts.Parameters == null) return;
                Logger.Debug(
                    $"Parameters: {string.Join(", ", opts.Parameters.AsParallel().Select(x => "'" + x + "'"))}");

                if (opts.Sync)
                {
                    if (opts.Search)
                    {
                        if (opts.Parameters.ToArray().Length == 1)
                            Search(opts.Parameters.First());
                        else
                            Console.WriteLine("Search requires only one package");
                    }
                    else
                    {
                        Install(opts.Parameters.ToArray());
                    }
                }

                if (!opts.GraphSerialization || opts.Sync || opts.Search) return;
                var array = opts.Parameters.ToArray();
                SerializeGraph(array[..^1], array[^1]);
            })
            .WithNotParsed(errs =>
            {
                var errors = errs as Error[] ?? errs.ToArray();

                if (errors.Length == 1 && errors.First() is HelpRequestedError or VersionRequestedError) return;
                Logger.Fatal(string.Join("\n", errors.Select(x => x)));
                Environment.Exit(-1);
            });
    }

    private static void UnhandledExceptionHook()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Logger.Error($"Object {sender.GetType().Name} trew:");
            Logger.Fatal(e.ExceptionObject);
        };
    }

    private static ColoredConsoleTarget ConfigureLogger(string configFilePath = "NLog.config",
        string targetName = "console")
    {
        var config = new XmlLoggingConfiguration(configFilePath);
        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();

        var target = config.FindTargetByName<ColoredConsoleTarget>(targetName);
        if (target == null)
            throw new KeyNotFoundException($"Target {targetName} not found or is not a ColoredConsoleTarget");
        return target;
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
        Parallel.ForEach(searchRes.Results, result =>
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
        });
    }

    private static void Install(string[] packages)
    {
        var flags = "-sif";
        List<DetailedPkgInfo> pkgExplicit = [];

        SearchExplicitPackages(packages, ref pkgExplicit);

        Console.WriteLine(
            $"AUR Explicit ({pkgExplicit.Count}): {string.Join(", ", pkgExplicit.AsParallel().Select(x => $"{x.Name}-{x.Version}"))}");

        flags = AskInstallFlags(flags);

        var inspector = new PackageInspector(Db.GetLocalDb());
        var table = inspector.GatherPackageInfo(pkgExplicit.Select(x => x.Name)).Result;

        var graph = Graph.BuildFor(pkgExplicit.Select(x => x.Name).AsParallel(), table);

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
        Logger.Debug($"Install order: {string.Join("->", installOrder)}");

        CloneRepos(installOrder, table);
        MakePackages(installOrder, flags);
    }

    // private static unsafe void UpdateAll()
    // {
    //     var localCache = DatabaseController.GetPackageCache(Db.GetLocalDb());
    //     var syncDbs = Db.GetSyncDbs();
    //     var syncCache = new AlpmList<AlpmPkgListNode>();
    //     foreach (var syncDb in syncDbs)
    //     {
    //         var pkgCache = DatabaseController.GetPackageCache(syncDb.Data);
    //         syncCache.AddRange(pkgCache);
    //     }
    //     
    //     var allCache = localCache.Extend(syncCache, ExtendOptions.DeleteEqual);
    //     var diff = allCache.Except(syncCache).ToList();
    //     
    //     Console.WriteLine($"diff: {string.Join(", ", diff.Select(x => DatabaseController.GetPackageName(x.Data)))}");
    //     Console.WriteLine(diff.Count);
    // }

    private static void SerializeGraph(IEnumerable<string> packages, string path)
    {
        Logger.Debug($"Serializing graph to '{path}'");
        if (!Directory.Exists(Path.GetDirectoryName(path))) return;

        var inspector = new PackageInspector(Db.GetLocalDb());
        var aurExplicit = packages as string[] ?? packages.ToArray();
        var table = inspector.GatherPackageInfo(aurExplicit).Result;

        var graph = Graph.BuildFor(aurExplicit, table);
        var algorithm = new GraphvizAlgorithm<string, Edge<string>>(graph);
        algorithm.FormatVertex += (_, args) => { args.VertexFormat.Label = args.Vertex; };
        algorithm.Generate(new FileDotEngine(), path);
    }

    private static void MakePackages(string[] installOrder, string flags)
    {
        foreach (var pkg in installOrder)
        {
            if (!Directory.Exists(pkg))
            {
                Logger.Fatal($"Package directory {Path.Combine(CachePath, pkg)} does not exist");
                Console.WriteLine($"ERROR: Could not find installation directory for pkg '{pkg}'");
                Environment.Exit(-1);
            }

            ShellRunner.Run($"makepkg -D {pkg} " + flags + " --needed --skippgpcheck");
        }
    }

    private static void CloneRepos(string[] installOrder, Hashtable table)
    {
        Parallel.ForEach(installOrder, pkg =>
        {
            var detailedPkgInfo = table[pkg] as DetailedPkgInfo;

            ShellRunner.Run(Directory.Exists(pkg)
                ? $"git -C {pkg} pull"
                : $"git clone https://aur.archlinux.org/{detailedPkgInfo!.Name}.git");
        });
    }

    private static void SearchExplicitPackages(IEnumerable<string> packages, ref List<DetailedPkgInfo> pkgExplicit)
    {
        var infoResult = Engine.Info(packages).Result;

        if (infoResult == null) return;

        pkgExplicit = infoResult.Results.ToList();
    }

    private static string AskInstallFlags(string flags)
    {
        flags += Input("Clean build? [Y/n]\n", "c", input =>
        {
            if (input != null && input.Equals("n", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            Logger.Debug("Adding -c to flags");
            return "c";
        });

        flags += Input("Remove make dependencies after installation? [y/N]\n", string.Empty, input =>
        {
            if (input == null || !input.Equals("y", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            Logger.Debug("Adding -r to flags");
            return "r";
        });
        return flags;
    }
}