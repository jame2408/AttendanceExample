namespace Attendance.Domain;

public record AttendanceStatus(AttendanceStatusEnum Status, string Reason, MangerProcessedTypeEnum ManagerProcessed);