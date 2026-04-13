using EkycService.Infrastructure.Logging;

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
            SafeLogger.Error<GlobalExceptionMiddleware>(ex, "Unhandled error");

            Console.WriteLine("EXCEPTION:");
            Console.WriteLine(ex.ToString());

            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsync("Internal Server Error");
        }
    }
}
