using Attendance.Service.DataContracts.Messages;
using Attendance.Service.DataContracts.Results;

namespace Attendance.Service.Interface.Service;

public interface IAttendanceRecordService
{
    Task<AttendanceClockResult> ClockAsync(AttendanceClockMessage message);
}