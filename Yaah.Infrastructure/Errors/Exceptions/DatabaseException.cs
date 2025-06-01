namespace Yaah.Infrastructure.Errors.Exceptions;

public class DatabaseException(string message) : Exception
{
    public override string Message { get; } = message;

    public DatabaseException(int errorCode) : this(ErrorTranslator.TranslateAlpmError(errorCode))
    {
    }

    public DatabaseException(IntPtr handle) : this(ErrorTranslator.TranslateAlpmError(handle))
    {
    }
}