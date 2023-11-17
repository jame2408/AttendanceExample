using System.Net;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Attendance.Api.Extensions;

public static class ValidationResultExtensions
{
    public static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult validationResult, PathString requestPath)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "Check the 'Errors' property for more details.",
            Instance = requestPath
        };

        return problemDetails;
    }
}