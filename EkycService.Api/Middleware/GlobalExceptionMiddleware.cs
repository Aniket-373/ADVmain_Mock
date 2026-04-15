using EkycService.Infrastructure.Logging;
using Serilog;

namespace EkycService.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            var correlationId = ctx.Items["CorrelationId"]?.ToString();

            Log.Error(ex,
                "ERROR: {Message} | Path: {Path}",
                ex.Message,
                ctx.Request.Path.Value
            );

            ctx.Response.StatusCode = 500;

            await ctx.Response.WriteAsJsonAsync(new
            {
                message = "Internal Server Error",
                correlationId
            });
        }
    }
}
