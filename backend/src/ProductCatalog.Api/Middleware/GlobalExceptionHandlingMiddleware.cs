using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        var response = context.Response;
        response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                var validationProblem = new ValidationProblemDetails(
                    validationException.Errors
                        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                        .ToDictionary(g => g.Key, g => g.ToArray()))
                {
                    Title = "Validation failed",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                };
                await response.WriteAsync(JsonSerializer.Serialize(validationProblem, jsonOptions));
                break;

            case EntityNotFoundException notFoundException:
                _logger.LogWarning(notFoundException, "Resource not found: {Message}", notFoundException.Message);
                response.StatusCode = (int)HttpStatusCode.NotFound;
                var notFoundProblem = new ProblemDetails
                {
                    Title = "Not Found",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = notFoundException.Message,
                    Instance = context.Request.Path
                };
                await response.WriteAsync(JsonSerializer.Serialize(notFoundProblem, jsonOptions));
                break;

            case DomainValidationException domainEx:
                _logger.LogWarning(domainEx, "Domain validation failure: {Message}", domainEx.Message);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                var domainProblem = new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = domainEx.Message,
                    Instance = context.Request.Path
                };
                await response.WriteAsync(JsonSerializer.Serialize(domainProblem, jsonOptions));
                break;

            default:
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var internalProblem = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "An unexpected error occurred while processing your request.",
                    Instance = context.Request.Path
                };
                await response.WriteAsync(JsonSerializer.Serialize(internalProblem, jsonOptions));
                break;
        }
    }
}