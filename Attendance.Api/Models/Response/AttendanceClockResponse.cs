using Attendance.Domain;
using Attendance.Service.DataContracts.Results;

namespace Attendance.Api.Models.Response;

public class AttendanceClockResponse
{
    public AttendanceClockResponse(AttendanceClockResult result)
    {
        Type = result.Type;
        ClockTime = result.ClockTime;
    }

    public AttendanceTypeEnum Type { get; }

    public DateTime ClockTime { get; }
}