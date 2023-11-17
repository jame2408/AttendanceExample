using Attendance.Domain;

namespace Attendance.Service.DataContracts.Messages;

public record struct AttendanceClockMessage(string EmployeeNumber, AttendanceTypeEnum Type, string IpAddress, string ComputerName, DateTime ClockTime);