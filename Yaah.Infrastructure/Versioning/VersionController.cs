using System.Text.RegularExpressions;
using static Yaah.Infrastructure.LibAlpm;

namespace Yaah.Infrastructure.Versioning;

public static partial class VersionController
{
    [GeneratedRegex("^(?<name>.+?)(?<op>>=|<=|=|>|<)(?<version>.+)$")]
    private static partial Regex PkgNameVersionRegex();

    /// <summary>
    ///     Compare two version strings and determine which one is 'newer'. Returns a value comparable to the way strcmp works.
    ///     Returns 1 if \p a is newer than \p b, 0 if a and b are the same version, or -1 if \p b is newer than \p a.
    ///     Different epoch values for version strings will override any further comparison. If no epoch is provided, 0 is
    ///     assumed.
    ///     Keep in mind that the pkgrel is only compared if it is available on both versions handed to this function. For
    ///     example, comparing 1.5-1 and 1.5 will yield 0; comparing 1.5-1 and 1.5-2 will yield -1 as expected. This is mainly
    ///     for supporting versioned dependencies that do not include the pkgrel.
    /// </summary>
    /// <see cref="https://man.archlinux.org/man/libalpm_packages.3.en" />
    public static int VersionComparison(string a, string b)
    {
        return alpm_pkg_vercmp(a, b);
    }

    public static Match? GetVersionMatch(string pkg)
    {
        var matches = PkgNameVersionRegex().Matches(pkg);
        return matches.Count == 0 ? null : matches[0];
    }
}