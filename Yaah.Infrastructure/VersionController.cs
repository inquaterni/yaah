using System.Text.RegularExpressions;

namespace Yaah.System;

public static partial class VersionController
{
    [GeneratedRegex("^(?<name>.+?)(?<op>>=|<=|=|>|<)(?<version>.+)$")]
    private static partial Regex PkgNameVersionRegex();
    
    public static Match? GetVersionMatch(string pkg)
    {
        var matches = PkgNameVersionRegex().Matches(pkg);
        return matches.Count == 0 ? null : matches[0];
    }
}