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

// using Bookify.Application.Exceptions;
// using Microsoft.AspNetCore.Mvc;
//
// namespace Bookify.Api.Middleware;
//
// public class ExceptionHandlingMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<ExceptionHandlingMiddleware> _logger;
//
//     public ExceptionHandlingMiddleware(
//         RequestDelegate next,
//         ILogger<ExceptionHandlingMiddleware> logger)
//     {
//         _next = next;
//         _logger = logger;
//     }
//
//     public async Task InvokeAsync(HttpContext context)
//     {
//         try
//         {
//             await _next(context);
//         }
//         catch (Exception exception)
//         {
//             _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
//
//             var exceptionDetails = GetExceptionDetails(exception);
//
//             var problemDetails = new ProblemDetails
//             {
//                 Status = exceptionDetails.Status,
//                 Type = exceptionDetails.Type,
//                 Title = exceptionDetails.Title,
//                 Detail = exceptionDetails.Detail,
//             };
//
//             if (exceptionDetails.Errors is not null)
//             {
//                 problemDetails.Extensions["errors"] = exceptionDetails.Errors;
//             }
//
//             context.Response.StatusCode = exceptionDetails.Status;
//
//             await context.Response.WriteAsJsonAsync(problemDetails);
//         }
//     }
//
//     private static ExceptionDetails GetExceptionDetails(Exception exception)
//     {
//         return exception switch
//         {
//             ValidationException validationException => new ExceptionDetails(
//                 StatusCodes.Status400BadRequest,
//                 "ValidationFailure",
//                 "Validation error",
//                 "One or more validation errors has occurred",
//                 validationException.Errors),
//             _ => new ExceptionDetails(
//                 StatusCodes.Status500InternalServerError,
//                 "ServerError",
//                 "Server error",
//                 "An unexpected error has occurred",
//                 null)
//         };
//     }
//
//     internal record ExceptionDetails(
//         int Status,
//         string Type,
//         string Title,
//         string Detail,
//         IEnumerable<object>? Errors);
// }