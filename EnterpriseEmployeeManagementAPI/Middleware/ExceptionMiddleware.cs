using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagementAPI.Middleware;

public sealed class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    private static readonly Action<ILogger, string, Exception?> DomainFailure =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(4000, nameof(DomainFailure)),
            "Request failed with a domain error: {Message}");

    private static readonly Action<ILogger, string, string, Exception?> UnhandledFailure =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(5000, nameof(UnhandledFailure)),
            "Unhandled exception while processing {Method} {Path}");

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private async Task WriteProblemDetailsAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Request conflict"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            UnhandledFailure(
                logger,
                context.Request.Method,
                context.Request.Path.Value ?? "/",
                exception);
        }
        else
        {
            DomainFailure(logger, exception.Message, exception);
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode < StatusCodes.Status500InternalServerError
                    ? exception.Message
                    : "Contact support with the supplied trace identifier.",
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier },
            },
            context.RequestAborted);
    }
}
