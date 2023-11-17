namespace Attendance.Domain;

public class AttendanceRecord
{
    private static readonly TimeOnly MorningClockInTime = new(9, 1, 0);
    private static readonly TimeOnly EveningClockOutTime = new(18, 30, 0);

    public Guid Id { get; init; }
    public string EmployeeNumber { get; }
    public AttendanceTypeEnum Type { get; }
    public string IpAddress { get; private set; }
    public string ComputerName { get; private set; }
    public DateTime? ClockTime { get; private set; }
    public AttendanceStatus AttendanceStatus { get; private set; } = null!;
    public static string LateClockInReason => "上班遲到";
    public static string EarlyClockOutReason => "下班早退";
    public static string NoClockInTimeReason  => "無打卡時間";

    private AttendanceRecord(string employeeNumber, AttendanceTypeEnum type, string ipAddress, string computerName,
        DateTime? clockTime)
    {
        EmployeeNumber = employeeNumber;
        Type = type;
        IpAddress = ipAddress;
        ComputerName = computerName;
        ClockTime = clockTime;
    }

    public static AttendanceRecord Create(string employeeNumber, AttendanceTypeEnum type, string ipAddress,
        string computerName, DateTime? clockTime)
    {
        var attendanceRecord = new AttendanceRecord(employeeNumber, type, ipAddress, computerName, clockTime)
        {
            Id = Guid.NewGuid(),
            AttendanceStatus = GetAttendanceStatus(type, clockTime)
        };

        return attendanceRecord;
    }

    public void UpdateClockOutRecord(string ipAddress, string computerName, DateTime clockTime)
    {
        if (Type == AttendanceTypeEnum.ClockOut)
        {
            IpAddress = ipAddress;
            ComputerName = computerName;
            ClockTime = clockTime;
            AttendanceStatus = GetAttendanceStatus(Type, clockTime);
        }
    }

    private static AttendanceStatus GetAttendanceStatus(AttendanceTypeEnum type, DateTime? clockTime)
    {
        if (clockTime.HasValue == false)
        {
            return new AttendanceStatus(AttendanceStatusEnum.Abnormal, NoClockInTimeReason, MangerProcessedTypeEnum.UnProcessed);
        }

        if (IsOnTime(clockTime.Value.TimeOfDay, type))
        {
            return new AttendanceStatus(AttendanceStatusEnum.Normal, string.Empty, MangerProcessedTypeEnum.UnProcessed);
        }

        var reason = type == AttendanceTypeEnum.ClockIn ? LateClockInReason : EarlyClockOutReason;
        return new AttendanceStatus(AttendanceStatusEnum.Abnormal, reason, MangerProcessedTypeEnum.UnProcessed);
    }

    private static bool IsOnTime(TimeSpan clockTimeOfDay, AttendanceTypeEnum type) =>
        type == AttendanceTypeEnum.ClockIn && clockTimeOfDay < MorningClockInTime.ToTimeSpan() ||
        type == AttendanceTypeEnum.ClockOut && clockTimeOfDay >= EveningClockOutTime.ToTimeSpan();
}