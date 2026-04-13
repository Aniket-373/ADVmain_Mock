using System.Diagnostics;

namespace EkycService.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;

        // Push into logging scope
        using (Serilog.Context.LogContext.PushProperty("correlation_id", correlationId))
        {
            await _next(context);
        }
    }
}