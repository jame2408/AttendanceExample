using Attendance.Domain;

namespace Attendance.Api.Models.Request;

public class AttendanceClockRequest
{
    public string EmployeeNumber { get; set; } = null!;

    public AttendanceTypeEnum Type { get; set; }

    public string IpAddress { get; set; } = null!;

    public string ComputerName { get; set; } = null!;

    public DateTime ClockTime { get; set; }
}