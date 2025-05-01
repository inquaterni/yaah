using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Database.Data;

namespace Database.Parser;

public interface IConverter
{
    public bool CanConvert(string input);
    public object Convert(string input);
}
public class StringByteSizeToLongConverter : IConverter
{
    public bool CanConvert(string input)
    {
        return input.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length == 2;
    }

    public object Convert(string input)
    {
        var data = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var (size, units) = (data[0].Trim(), data[1].Trim());
        if (float.TryParse(size, out var sizeF))
        {
            return units switch
            {
                "B" => (long)sizeF,
                "b" => (long)sizeF,
                "KiB" => (long)(sizeF * 1024),
                "MiB" => (long)(sizeF * 1024 * 1024),
                "GiB" => (long)(sizeF * 1024 * 1024 * 1024),
                "TiB" => (long)(sizeF * 1024 * 1024 * 1024 * 1024),
                _ => throw new ArgumentOutOfRangeException(nameof(input))
            };
        }
        throw new NotSupportedException();
    }
}
public class DatetimeConverter : IConverter
{
    private const string Format = "ddd dd MMM yyyy hh:mm:ss tt";

    public bool CanConvert(string input)
    {
        return DateTime.TryParseExact(input[..input.LastIndexOf(' ')], Format, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _);
    }

    public object Convert(string input)
    {
        return DateTime.ParseExact(input[..input.LastIndexOf(' ')], Format, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class StdoutLabelAttribute : Attribute
{
    public string Label { get; }
    public IConverter? TypeConverter { get; }

    public StdoutLabelAttribute(string label, Type? typeConverter = null)
    {
        Label = label;
        
        if (typeConverter != null)
        {
            TypeConverter = Activator.CreateInstance(typeConverter) as IConverter;
        }
        else TypeConverter = null;
    }
}

public static class PkgInfoParser
{
    public static PkgInfo? Parse(string stdout)
    {
        var subStrPos = stdout.IndexOf(':');
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var props = typeof(PkgInfo).GetProperties();
        var info = new PkgInfo();
        try
        {
            Parallel.ForEach(props, (prop) =>
            {
                var attr = prop.GetCustomAttribute<StdoutLabelAttribute>();

                string line;
                if (attr != null)
                {
                    line = lines.AsParallel().FirstOrDefault(l => l.Trim().StartsWith(attr.Label)) ?? string.Empty;
                }
                else
                {
                    line = lines.AsParallel().FirstOrDefault(l => l.Trim().StartsWith(prop.Name)) ?? string.Empty;
                }
                
                var res = line.Substring(subStrPos + 2, line.Length - subStrPos - 2);

                if (prop.PropertyType.IsArray)
                {
                    prop.SetValue(info, res.Split("  ", StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    if (attr is { TypeConverter: not null })
                    {
                        if (attr.TypeConverter.CanConvert(res))
                        {
                            var value = Convert.ChangeType(attr.TypeConverter.Convert(res), prop.PropertyType);
                            prop.SetValue(info, value);
                        }
                        else
                        {
                            throw new FormatException($"Cannot convert value '{res}' to type {prop.PropertyType}");
                        }
                    }
                    else
                    {
                        prop.SetValue(info, res);
                    }
                }
            });
        }
        catch (AggregateException)
        {
            return null;
        }
        return info;
    }
}