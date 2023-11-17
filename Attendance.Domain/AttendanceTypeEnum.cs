using System.ComponentModel;

namespace Attendance.Domain;

public enum AttendanceTypeEnum
{
    [Description("上班打卡")]
    ClockIn = 1,
    [Description("下班打卡")]
    ClockOut = 2
}