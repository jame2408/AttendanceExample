using System.Net;
using FluentValidation;

namespace Attendance.Api.Models.Request.Validator;

public class AttendanceClockRequestValidator : AbstractValidator<AttendanceClockRequest>
{
    public AttendanceClockRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber)
            .Length(6)
            .Matches(@"^\d{6}$")
            .WithMessage("Employee number must be 6 digits");
        
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid attendance type");

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .WithMessage("IP address is required")
            .Must(BeValidIpAddress)
            .WithMessage("Invalid IP address");
        
        RuleFor(x => x.ComputerName)
            .NotEmpty()
            .WithMessage("Computer name is required")
            .Length(8)
            .WithMessage("Computer name must be 8 characters")
            .Must(m => m.StartsWith("NM"))
            .WithMessage("Computer name must start with 'NM'");
        
        RuleFor(x => x.ClockTime)
            .Must(clockTime => clockTime.Date == DateTime.Today)
            .WithMessage("Clock time must be today");
    }
    
    private bool BeValidIpAddress(string ipAddress) => IPAddress.TryParse(ipAddress, out _);
}