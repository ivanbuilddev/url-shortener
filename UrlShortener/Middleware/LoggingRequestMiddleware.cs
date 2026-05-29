namespace UrlShortener.Middleware;

public class LoggingRequestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingRequestMiddleware> _logger;

    public LoggingRequestMiddleware(ILogger<LoggingRequestMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await _next(context);
        _logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
    }
}