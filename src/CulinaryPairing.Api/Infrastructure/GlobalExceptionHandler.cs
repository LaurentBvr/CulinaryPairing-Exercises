using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Ardalis.Result.FluentValidation;
using CulinaryPairing.Infrastructure.Correlation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryPairing.Api.Infrastructure;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment,
    IServiceScopeFactory scopeFactory) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = "unknown";
        try
        {
            var provider = httpContext.RequestServices.GetService<ICorrelationIdProvider>();
            if (provider != null) correlationId = provider.CorrelationId;
        }
        catch { }

        logger.LogError(exception,
            "Unhandled exception. CorrelationId: {CorrelationId}, Path: {Path}",
            correlationId, httpContext.Request.Path);

        if (exception is FluentValidation.ValidationException validationException)
        {
            var errors = new ValidationResult(validationException.Errors).AsErrors();
            var result = Result.Invalid(errors).ToMinimalApiResult();
            await result.ExecuteAsync(httpContext);
            return true;
        }

        var (statusCode, title, type) = exception switch
        {
            UnauthorizedAccessException => (403, "Acces refuse.",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"),
            _ => (500, "Une erreur interne est survenue.",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type
        };

        if (environment.IsDevelopment())
            problemDetails.Detail = exception.Message;

        problemDetails.Extensions["correlationId"] = correlationId;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}