using System.Net;
using System.Text.Json;
using AiCoachOs.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Middleware;

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            NotFoundException => HttpStatusCode.NotFound,
            ConflictException => HttpStatusCode.Conflict,
            ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        response.StatusCode = (int)statusCode;

        object responseBody;

        if (exception is ValidationException validationEx)
        {
            responseBody = new ValidationProblemDetails(validationEx.Errors)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Error",
                Detail = validationEx.Message
            };
        }
        else
        {
            responseBody = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = GetTitle(statusCode),
                Detail = exception.Message
            };
        }

        var json = JsonSerializer.Serialize(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await response.WriteAsync(json);
    }

    private static string GetTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.Conflict => "Conflict",
        _ => "Internal Server Error"
    };
}
