using System.Net;
using Attendance.Api.Controllers;
using Attendance.Api.Helpers;
using Attendance.Api.Models.Request;
using Attendance.Api.Models.Response;
using Attendance.Domain;
using Attendance.Service.DataContracts.Messages;
using Attendance.Service.DataContracts.Results;
using Attendance.Service.Interface.Service;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Attendance.Tests.Api.Controllers;

[TestClass]
public class AttendanceRecordControllerTests
{
    private IAttendanceRecordService _service = null!;
    private AttendanceRecordController _controller = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Mocking FluentValidation
        var validationService = Substitute.For<IValidationService>();
        validationService.ValidateAsync(Arg.Any<AttendanceClockRequest>())
            .Returns(new ValidationResult()); // No Errors, IsValid = true.

        _service = Substitute.For<IAttendanceRecordService>();
        _controller = new AttendanceRecordController(_service, validationService);
    }
    
    [TestMethod]
    public async Task Clock_ValidRequest_ReturnsOkWithResult()
    {
        // Arrange
        var request = new AttendanceClockRequest
        {
            EmployeeNumber = "388496",
            Type = AttendanceTypeEnum.ClockIn,
            IpAddress = "192.168.0.1",
            ComputerName = "NM388496",
            ClockTime = new DateTime(2023, 7, 31, 09, 00, 59)
        };

        var clockResult = new AttendanceClockResult(AttendanceTypeEnum.ClockIn, request.ClockTime);
        _service.ClockAsync(Arg.Is<AttendanceClockMessage>(
                message =>
                    message.EmployeeNumber == request.EmployeeNumber &&
                    message.Type == request.Type &&
                    message.IpAddress == request.IpAddress &&
                    message.ComputerName == request.ComputerName &&
                    message.ClockTime == request.ClockTime))
            .Returns(clockResult);
        
        // Act
        var actionResult = await _controller.Clock(request);
        
        // Assert
        if (actionResult.Result is OkObjectResult result)
        {
            result.StatusCode.Should().Be((int)HttpStatusCode.OK); 
            if (result.Value is AttendanceClockResponse response)
            {
                response.Type.Should().Be(AttendanceTypeEnum.ClockIn);
                response.ClockTime.Should().Be(new DateTime(2023, 7, 31, 09, 00, 59));
            }
            else
            {
                Assert.Fail("Result is not AttendanceClockResponse");
            }
        }
        else
        {
            Assert.Fail("Result is not OkObjectResult");
        }
    }
    
    [TestMethod]
    public async Task Clock_ValidClockInRequest_CallsClockInAsync()
    {
        // Arrange
        var request = new AttendanceClockRequest { Type = AttendanceTypeEnum.ClockIn };

        // Act
        await _controller.Clock(request);

        // Assert
        await _service.Received(1).ClockAsync(Arg.Any<AttendanceClockMessage>());
    }

    [TestMethod]
    public async Task Clock_ValidClockOutRequest_CallsClockOutAsync()
    {
        // Arrange
        var request = new AttendanceClockRequest { Type = AttendanceTypeEnum.ClockOut };

        // Act
        await _controller.Clock(request);

        // Assert
        await _service.Received(1).ClockAsync(Arg.Any<AttendanceClockMessage>());
    }
}