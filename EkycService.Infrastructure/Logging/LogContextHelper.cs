using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Context;

namespace EkycService.Infrastructure.Logging;

public static class LogContextHelper
{
    public static IDisposable PushClass(string className)
    {
        return LogContext.PushProperty("flow_class", className);
    }
}