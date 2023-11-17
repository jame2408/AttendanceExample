using Attendance.Domain;

namespace Attendance.Service.DataContracts.Results;

public record struct AttendanceClockResult(AttendanceTypeEnum Type, DateTime ClockTime);