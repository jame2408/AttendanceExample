using System.Net;
using Attendance.Api.Extensions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Attendance.Tests.Api.Extensions;

[TestClass]
public class ValidationResultExtensionsTests
{
    [TestMethod]
    public void ToProblemDetails_ShouldReturnCorrectProblemDetails()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("Property1", "Error0"),
            new("Property1", "Error1"),
            new("Property2", "Error2")
        };
        var validationResult = new ValidationResult(validationFailures);
        var requestPath = new PathString("/test");

        // Act
        var problemDetails = validationResult.ToValidationProblemDetails(requestPath);

        // Assert
        problemDetails.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Be("Check the 'Errors' property for more details.");
        problemDetails.Instance.Should().Be(requestPath);

        var errors = problemDetails.Errors;
        errors.Should().NotBeNull();
        errors.Should().BeEquivalentTo(new Dictionary<string, string[]>()
        {
            ["Property1"] = new []{ "Error0", "Error1" },
            ["Property2"] = new []{ "Error2" }
        });
    }
}