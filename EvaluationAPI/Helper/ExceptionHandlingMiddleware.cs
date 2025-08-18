using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using EA.Common;

namespace EvaluationAPI.Helper;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpStatusCodeException ex) // custom exception
        {
            _logger.LogError(ex, "Custom error occurred: {Message}", ex.Message);
            await HandleCustomExceptionAsync(context, ex);
        }
        catch (Exception ex) // unhandled exceptions
        {
            _logger.LogError(ex, "Unhandled error occurred.");
            await HandleGlobalExceptionAsync(context, ex);
        }
    }
    private Task HandleGlobalExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            errorMessage = "An unexpected error occurred. Please try again later.",
            code = "INTERNAL_SERVER_ERROR",
            data = new { detail = ex.Message }
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
    
    private Task HandleCustomExceptionAsync(HttpContext context, HttpStatusCodeException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex.StatusCode;

        var response = new
        {
            statusCode = ex.StatusCode,
            errorMessage = ex.Message,
            code = ex.Code,
            data = ex.Data
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
