using CommandLine;

namespace Yaah.CLI;

internal class Options
{
    [Option('S', "sync", HelpText = "Install or update the specified packages")]
    public bool Sync { get; set; }

    [Option('s', "search", HelpText = "Search for specified packages")]
    public bool Search { get; set; }
    // [Option('u', "update-all", HelpText = "Update all AUR packages")]
    // public bool Update { get; set; }

    [Option('D', "debug", HelpText = "Enable debug output")]
    public bool Debug { get; set; }

    [Option('g', "graph-serialization", HelpText = "Enable graph serialization")]
    public bool GraphSerialization { get; set; }

    [Value(0, MetaName = "parameters", Required = true, HelpText = "Variable parameters", Min = 1)]
    public IEnumerable<string>? Parameters { get; set; }
}