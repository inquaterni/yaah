using System.Runtime.InteropServices;
using static Yaah.Infrastructure.Alpm.LibAlpm;

namespace Yaah.Infrastructure.Errors;

public static class ErrorTranslator
{
    /// <summary>
    ///  This error has maximum numeric value in libalpm
    /// </summary>
    private const int MissingCapabilitySignatures = 54;
    public static int GetErrorFromHandle(IntPtr handle)
    {
        return alpm_errno(handle);
    }
    public static string TranslateAlpmError(int errCode)
    {
        if (errCode > MissingCapabilitySignatures)
        {
            return "Unknown error. This error is not related to libalpm.";
        }
        
        var ptr = alpm_strerror(errCode);
        if (ptr == IntPtr.Zero)
        {
            return $"error code {errCode}";
        }
        var result = Marshal.PtrToStringAnsi(ptr);
        return string.IsNullOrEmpty(result) ? $"error code {errCode}" : result;
    }

    public static string TranslateAlpmError(IntPtr handle)
    {
        return TranslateAlpmError(GetErrorFromHandle(handle));
    }
}