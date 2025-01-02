using Bookify.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            context.Response.ContentType = "application/json";

            if (exception is ValidationException validationException)
            {
                var validationProblemDetails = CreateValidationProblemDetails(validationException);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(validationProblemDetails);
            }
            else
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "ServerError",
                    Title = "Server Error",
                    Detail = "An unexpected error has occurred."
                };

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }

    private ValidationProblemDetails CreateValidationProblemDetails(ValidationException validationException)
    {
        var validationErrors = new Dictionary<string, string[]>();

        foreach (var error in validationException.Errors)
        {
            if (validationErrors.ContainsKey(error.PropertyName))
            {
                var errorList = validationErrors[error.PropertyName].ToList();
                errorList.Add(error.ErrorMessage);
                validationErrors[error.PropertyName] = errorList.ToArray();
            }
            else
                validationErrors.Add(error.PropertyName, [error.ErrorMessage]);
        }

        var validationProblemDetails = new ValidationProblemDetails(validationErrors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "ValidationFailure",
            Title = "Validation Error",
            Detail = "One or more validation errors have occurred."
        };

        return validationProblemDetails;
    }
}