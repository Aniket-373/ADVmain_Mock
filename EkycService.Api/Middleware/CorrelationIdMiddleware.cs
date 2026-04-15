using Serilog.Context;

namespace EkycService.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Try get from request header
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();

        // 2. If not present → generate
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // 3. Store in context
        context.Items["CorrelationId"] = correlationId;

        // 4. Add to response header (VERY IMPORTANT)
        context.Response.Headers["x-correlation-id"] = correlationId;

        // 5. Push into Serilog context
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}