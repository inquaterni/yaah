using Database;
using Database.Parser;

namespace Testing;

public class PkgInfoParserTest
{
    [Theory]
    [InlineData("gtk3", "gtk3")]
    public void Parse_ReturnsCorrectPkgName(string pkg, string expected)
    {
        var stdout = DatabaseController.RunProcessGetStdout("pacman -Qi " + pkg);
        var info = PkgInfoParser.Parse(stdout);
        Assert.Equal(expected, info!.Name);
        Assert.IsType<long>(info!.InstalledSize);
    }

    [Theory]
    [InlineData("pkg")]
    public void Parse_ReturnsNullIncorrectInput(string pkg)
    {
        var stdout = DatabaseController.RunProcessGetStdout("pacman -Qi " + pkg);
        var info = PkgInfoParser.Parse(stdout);
        Assert.Null(info);
    }
    
}