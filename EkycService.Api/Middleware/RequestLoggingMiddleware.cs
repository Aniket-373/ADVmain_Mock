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

        SafeLogger.Info<RequestLoggingMiddleware>(
            "Request {Method} {Path}",
            ctx.Request.Method,
            ctx.Request.Path);

        await _next(ctx);

        sw.Stop();

        SafeLogger.Info<RequestLoggingMiddleware>(
            "Response {Status} {Time}ms",
            ctx.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
