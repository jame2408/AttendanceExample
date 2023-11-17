using Attendance.Api.Models.Request;
using Attendance.Api.Models.Request.Validator;
using Attendance.Domain;
using FluentValidation.TestHelper;

namespace Attendance.Tests.Api.Validator;

[TestClass]
public class AttendanceClockRequestValidatorTests
{
    private readonly AttendanceClockRequestValidator _validator = new();

    [TestMethod]
    public void EmployeeNumber_ShouldBe6Digits()
    {
        var model = new AttendanceClockRequest { EmployeeNumber = "12345", Type = AttendanceTypeEnum.ClockIn, IpAddress = "192.168.0.1", ComputerName = "NM123456", ClockTime = DateTime.Today };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeNumber)
            .WithErrorMessage("Employee number must be 6 digits");

        model.EmployeeNumber = "1234567";
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeNumber)
            .WithErrorMessage("Employee number must be 6 digits");

        model.EmployeeNumber = "123456";
        result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.EmployeeNumber);
    }

    [TestMethod]
    public void Type_ShouldBeInEnum()
    {
        var model = new AttendanceClockRequest { EmployeeNumber = "123456", Type = (AttendanceTypeEnum)10, IpAddress = "192.168.0.1", ComputerName = "NM123456", ClockTime = DateTime.Today };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Invalid attendance type");

        model.Type = AttendanceTypeEnum.ClockIn;
        result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);

        model.Type = AttendanceTypeEnum.ClockOut;
        result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [TestMethod]
    public void IpAddress_ShouldBeValidAndRequired()
    {
        var model = new AttendanceClockRequest { EmployeeNumber = "123456", Type = AttendanceTypeEnum.ClockIn, IpAddress = "", ComputerName = "NM123456", ClockTime = DateTime.Today };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
            .WithErrorMessage("IP address is required");

        model.IpAddress = "invalid ip";
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
            .WithErrorMessage("Invalid IP address");

        model.IpAddress = "192.168.0.1";
        result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.IpAddress);
    }

    [TestMethod]
    public void ComputerName_ShouldBeValidAndRequired()
    {
        var model = new AttendanceClockRequest { EmployeeNumber = "123456", Type = AttendanceTypeEnum.ClockIn, IpAddress = "192.168.0.1", ComputerName = "", ClockTime = DateTime.Today };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ComputerName)
            .WithErrorMessage("Computer name is required");

        model.ComputerName = "NM123";
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ComputerName)
            .WithErrorMessage("Computer name must be 8 characters");

        model.ComputerName = "12345678";
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ComputerName)
            .WithErrorMessage("Computer name must start with 'NM'");

        model.ComputerName = "NM123456";
        result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.ComputerName);
    }

    [TestMethod]
    public void ClockTime_ShouldBeToday()
    {
        var model = new AttendanceClockRequest { EmployeeNumber = "123456", Type = AttendanceTypeEnum.ClockIn, IpAddress = "192.168.0.1", ComputerName = "NM123456", ClockTime = DateTime.Today.AddDays(-1) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ClockTime)
            .WithErrorMessage("Clock time must be today");

        model.ClockTime = DateTime.Today;
        result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.ClockTime);
    }
}