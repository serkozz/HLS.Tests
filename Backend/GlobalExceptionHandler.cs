using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        ProblemDetails pd;


        if (exception is BadHttpRequestException badRequestEx)
        {
            pd = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Request Parameter",
                Detail = exception.Message, // Здесь будет "Required parameter..."
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(pd, cancellationToken);
            return true;
        }
        
        _logger.LogError(exception, "An unhandled exception occurred.");

        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Input"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        httpContext.Response.StatusCode = statusCode;

        pd = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(pd, cancellationToken);

        return true; 
    }
}