using Attendance.Domain;

namespace Attendance.Service.Interface.Repository;

public interface IAttendanceRecordRepository
{
    Task<IReadOnlyCollection<AttendanceRecord>> GetByEmployeeNumberAndDateAsync(string messageEmployeeNumber, DateTime clockTimeDate);
    Task SaveAsync(AttendanceRecord attendanceRecord);
    Task SaveAsync(IEnumerable<AttendanceRecord> attendanceRecords);
}