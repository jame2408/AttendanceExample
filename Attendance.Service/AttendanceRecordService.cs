using Attendance.Domain;
using Attendance.Service.DataContracts.Messages;
using Attendance.Service.DataContracts.Results;
using Attendance.Service.Exception;
using Attendance.Service.Interface.Repository;
using Attendance.Service.Interface.Service;

namespace Attendance.Service;

public class AttendanceRecordService : IAttendanceRecordService
{
    private readonly IAttendanceRecordRepository _attendanceRecordRepository;

    public AttendanceRecordService(IAttendanceRecordRepository attendanceRecordRepository)
    {
        _attendanceRecordRepository = attendanceRecordRepository;
    }

    public Task<AttendanceClockResult> ClockAsync(AttendanceClockMessage message) =>
        message.Type switch
        {
            AttendanceTypeEnum.ClockIn => ClockInAsync(message),
            AttendanceTypeEnum.ClockOut => ClockOutAsync(message),
            _ => throw new InvalidAttendanceTypeException("Message type must be ClockIn or ClockOut.", nameof(message))
        };

    private async Task<AttendanceClockResult> ClockInAsync(AttendanceClockMessage message)
    {
        var existingRecord =
            await _attendanceRecordRepository.GetByEmployeeNumberAndDateAsync(message.EmployeeNumber,
                message.ClockTime.Date);
        if (existingRecord != null && existingRecord.Any(record => record is { Type: AttendanceTypeEnum.ClockIn, ClockTime: not null }))
        {
            throw new ClockInTwiceException("Cannot clock in twice in the same day.");
        }

        var attendanceRecord = AttendanceRecord.Create(
            message.EmployeeNumber,
            message.Type,
            message.IpAddress,
            message.ComputerName,
            message.ClockTime);

        await _attendanceRecordRepository.SaveAsync(attendanceRecord);

        return new AttendanceClockResult(attendanceRecord.Type, attendanceRecord.ClockTime!.Value);
    }

    private async Task<AttendanceClockResult> ClockOutAsync(AttendanceClockMessage message)
    {
        var recordsToSave = new List<AttendanceRecord>();

        // Get all records for the employee on the same day.
        var existingRecords =
            await _attendanceRecordRepository.GetByEmployeeNumberAndDateAsync(message.EmployeeNumber,
                message.ClockTime.Date);

        // Find the existing clock in and clock out records.
        var existingClockInRecord = existingRecords.FirstOrDefault(record => record.Type == AttendanceTypeEnum.ClockIn);
        var existingClockOutRecord =
            existingRecords.FirstOrDefault(record => record.Type == AttendanceTypeEnum.ClockOut);

        // if there is no clock in record, create one, and status is abnormal.
        if (existingClockInRecord == null)
        {
            var clockInRecord = AttendanceRecord.Create(
                message.EmployeeNumber,
                AttendanceTypeEnum.ClockIn,
                message.IpAddress,
                message.ComputerName,
                null); // 無打卡紀錄的打卡時間為 null

            recordsToSave.Add(clockInRecord);
        }

        // if there is a clock out record, update it, else create one.
        if (existingClockOutRecord != null)
        {
            existingClockOutRecord.UpdateClockOutRecord(message.IpAddress, message.ComputerName, message.ClockTime);
            recordsToSave.Add(existingClockOutRecord);
        }
        else
        {
            var clockOutRecord = AttendanceRecord.Create(
                message.EmployeeNumber,
                message.Type,
                message.IpAddress,
                message.ComputerName,
                message.ClockTime);

            recordsToSave.Add(clockOutRecord);
        }

        // Save all records.
        await _attendanceRecordRepository.SaveAsync(recordsToSave);

        var resultRecord = recordsToSave.First(record => record.Type == AttendanceTypeEnum.ClockOut);
        return new AttendanceClockResult(resultRecord.Type, resultRecord.ClockTime!.Value);
    }
}