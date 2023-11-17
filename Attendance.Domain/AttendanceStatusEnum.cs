using System.ComponentModel;

namespace Attendance.Domain;

public enum AttendanceStatusEnum
{
    [Description("正常的出勤紀錄")]
    Normal = 0,
    [Description("異常的出勤紀錄")]
    Abnormal = 1
}