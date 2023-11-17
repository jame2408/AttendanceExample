using System.Net;
using System.Text.Json;
using Attendance.Api.Middlewares;
using Attendance.Service.Exception;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Attendance.Tests.Api.Middleware;

[TestClass]
public class ExceptionMiddlewareTests
{
    private ExceptionMiddleware _middleware = null!;
    private RequestDelegate _nextDelegate = null!;
    private ILogger<ExceptionMiddleware> _logger = null!;
    private ProblemDetailsFactory _problemDetailsFactory = null!;

    [TestInitialize]
    public void Setup()
    {
        _nextDelegate = NSubstitute.Substitute.For<RequestDelegate>();
        _logger = NSubstitute.Substitute.For<ILogger<ExceptionMiddleware>>();
        _problemDetailsFactory = NSubstitute.Substitute.For<ProblemDetailsFactory>();
        _problemDetailsFactory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>()).Returns(new ProblemDetails());

        _middleware = new ExceptionMiddleware(_nextDelegate, _logger, _problemDetailsFactory);
    }

    [TestMethod]
    public async Task InvokeAsync_WhenExceptionOccurs_ReturnsProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;
        context.Request.Path = new PathString("/test");

        _nextDelegate.Invoke(Arg.Any<HttpContext>())
            .Returns(Task.FromException(new InvalidAttendanceTypeException("Test2", "Test1")));

        // Act
        await _middleware.InvokeAsync(context);
        
        // Reset stream position to read it
        memoryStream.Position = 0;
        var reader = new StreamReader(memoryStream);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        // Assert
        problemDetails!.Title.Should().Be("Invalid attendance type");
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Be("Test2 (Parameter 'Test1')");
        problemDetails.Instance.Should().Be("/test");
        
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        _logger.Received().LogWarning(Arg.Any<InvalidAttendanceTypeException>(), "Invalid attendance type");
    }
}