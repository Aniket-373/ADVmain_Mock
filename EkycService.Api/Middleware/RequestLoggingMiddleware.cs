using System.Diagnostics;
using EkycService.Infrastructure.Logging;

namespace EkycService.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var sw = Stopwatch.StartNew();

        var correlationId = ctx.Items["CorrelationId"]?.ToString();

        SafeLogger.Info<RequestLoggingMiddleware>(
            "HTTP_REQUEST",
            new
            {
                type = "request",
                method = ctx.Request.Method,
                path = ctx.Request.Path,
                correlation_id = correlationId
            });

        await _next(ctx);

        sw.Stop();

        SafeLogger.Info<RequestLoggingMiddleware>(
            "HTTP_RESPONSE",
            new
            {
                type = "response",
                status = ctx.Response.StatusCode,
                duration_ms = sw.ElapsedMilliseconds,
                correlation_id = correlationId
            });
    }
}
