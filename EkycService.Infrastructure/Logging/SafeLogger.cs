using Serilog;
using EkycService.Api.Helpers;

namespace EkycService.Infrastructure.Logging;

public static class SafeLogger
{
    public static void Info<T>(string message, object data)
    {
        Log.ForContext("class_name", typeof(T).Name)
           .ForContext("log_type", "application")
           .Information("{@Data}", data);
    }

    public static void Error<T>(Exception ex, string message, object data = null)
    {
        Log.ForContext("class_name", typeof(T).Name)
           .ForContext("log_type", "error")
           .Error(ex, "{@Data}", data);
    }
}