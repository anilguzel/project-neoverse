using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Neoverse.ApiBase.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString();
        _logger.LogInformation("Handling {Method} {Path} - Trace:{TraceId}", context.Request.Method, context.Request.Path, traceId);
        await _next(context);
        _logger.LogInformation("Response {StatusCode} - Trace:{TraceId}", context.Response.StatusCode, traceId);
    }
}
