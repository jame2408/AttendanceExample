using Attendance.Domain;
using Attendance.Service.Interface.Repository;

namespace Attendance.Infrastructure;

public class AttendanceRecordRepository : IAttendanceRecordRepository
{
    public async Task<IReadOnlyCollection<AttendanceRecord>> GetByEmployeeNumberAndDateAsync(
        string messageEmployeeNumber, DateTime clockTimeDate)
    {
        return new List<AttendanceRecord>()
        {
            // AttendanceRecord.Create("388496", AttendanceTypeEnum.ClockIn, "8.8.8.8", "NM388496",
            //     new DateTime(2023, 7, 28, 08, 30, 00))
        };
    }

    public Task SaveAsync(AttendanceRecord attendanceRecord)
    {
        return Task.CompletedTask;
    }

    public Task SaveAsync(IEnumerable<AttendanceRecord> attendanceRecords)
    {
        return Task.CompletedTask;
    }
}