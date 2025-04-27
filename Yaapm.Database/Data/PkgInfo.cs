using Database.Parser;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Database.Data;

public sealed class PkgInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    public string Architecture { get; set; }
    [StdoutLabel("URL")]
    public string Url { get; set; }
    public string[] Licenses { get; set; }
    public string[] Groups { get; set; }
    public string[] Provides { get; set; }
    [StdoutLabel("Depends On")]
    public string[] Depends { get; set; }
    [StdoutLabel("Optional Deps")]
    public string[] OptDepends { get; set; }
    [StdoutLabel("Required By")]
    public string[] RequiredBy { get; set; }
    [StdoutLabel("Optional For")]
    public string[] OptionalFor { get; set; }
    [StdoutLabel("Conflicts With")]
    public string[] Conflicts { get; set; }
    public string[] Replaces { get; set; }
    [StdoutLabel("Installed Size", typeof(StringByteSizeToLongConverter))]
    public long InstalledSize { get; set; }
    public string Packager { get; set; }
    [StdoutLabel("Build Date", typeof(DatetimeConverter))]
    public DateTime BuildDate { get; set; }
    [StdoutLabel("Install Date", typeof(DatetimeConverter))]
    public DateTime InstallDate { get; set; }
    [StdoutLabel("Install Reason")]
    public string InstallReason { get; set; }
    [StdoutLabel("Install Script")]
    public string InstallScript { get; set; }
    [StdoutLabel("Validated By")]
    public string ValidatedBy { get; set; }
}