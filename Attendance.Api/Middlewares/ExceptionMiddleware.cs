using System.Net;
using Attendance.Service.Exception;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Attendance.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _next = next;
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // In this, you can add other handler, ex: add log or send email...

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(context);
        problemDetails.Instance = context.Request.Path;

        switch (exception)
        {
            case InvalidAttendanceTypeException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                SetProblemDetails(problemDetails, (int)HttpStatusCode.BadRequest, "Invalid attendance type",
                    exception.Message);

                _logger.LogWarning(exception, "Invalid attendance type");
                break;
            case ClockInTwiceException:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                SetProblemDetails(problemDetails, (int)HttpStatusCode.UnprocessableEntity, "Clock in twice",
                    exception.Message);

                _logger.LogWarning(exception, "Clock in twice");
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                SetProblemDetails(problemDetails, (int)HttpStatusCode.InternalServerError, "Internal Server Error",
                    "An unexpected error occurred.");

                _logger.LogError(exception, "An unexpected error occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path, context.Request.Method);
                break;
        }

        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private void SetProblemDetails(ProblemDetails details, int statusCode, string title, string detail)
    {
        details.Status = statusCode;
        details.Title = title;
        details.Detail = detail;
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}