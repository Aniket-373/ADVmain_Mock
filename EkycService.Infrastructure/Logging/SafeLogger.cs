using Serilog;
using EkycService.Api.Helpers;

namespace EkycService.Infrastructure.Logging;

public static class SafeLogger
{
    public static void Info<T>(string template, params object[] args)
    {
        Log.ForContext("class_name", typeof(T).Name)
           .Information(MaskingHelper.Mask(template), args);
    }

    public static void Error<T>(Exception ex, string template, params object[] args)
    {
        Log.ForContext("class_name", typeof(T).Name)
           .Error(ex, MaskingHelper.Mask(template), args);
    }
}