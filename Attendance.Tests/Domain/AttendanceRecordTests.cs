using Attendance.Domain;

namespace Attendance.Tests.Domain;

[TestClass]
public class AttendanceRecordTests
{
    private static AttendanceRecord _attendanceRecord = null!;
    private const string EmployeeNumber = "388496";
    private static readonly (string ipAddress, string computerName) FirstComputer = ("192.168.0.1", "NM388496");
    private static readonly (string ipAddress, string computerName) SecondComputer = ("192.168.0.2", "NM421331");

    [TestMethod]
    public void Given_ValidClockInTime_When_CreateAttendanceRecord_Then_RecordIsNormal()
    {
        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockIn, new DateTime(2023, 7, 28, 09, 00, 59));

        AttendanceStatusShouldBe(AttendanceStatusEnum.Normal);
    }

    [TestMethod]
    public void Given_LateClockInTime_When_CreateAttendanceRecord_Then_RecordIsAbnormal()
    {
        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockIn, new DateTime(2023, 7, 28, 09, 01, 00));

        AttendanceStatusShouldBe(AttendanceStatusEnum.Abnormal, AttendanceRecord.LateClockInReason);
    }

    [TestMethod]
    public void Given_EarlyClockOutTime_When_CreateAttendanceRecord_Then_RecordIsAbnormal()
    {
        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockOut, new DateTime(2023, 7, 28, 18, 29, 59));

        AttendanceStatusShouldBe(AttendanceStatusEnum.Abnormal, AttendanceRecord.EarlyClockOutReason);
    }

    [TestMethod]
    public void Given_ValidClockOutTime_When_CreateAttendanceRecord_Then_RecordIsNormal()
    {
        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockOut, new DateTime(2023, 7, 28, 18, 30, 00));

        AttendanceStatusShouldBe(AttendanceStatusEnum.Normal);
    }

    [TestMethod]
    public void Given_ValidClockOutTime_When_UpdateClockOutRecord_Then_RecordIsUpdated()
    {
        var initialTime = new DateTime(2023, 7, 28, 18, 30, 00);
        var updatedTime = new DateTime(2023, 7, 28, 19, 00, 00);

        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockOut, initialTime);
        AttendanceStatusShouldBe(AttendanceStatusEnum.Normal);

        WhenUpdateClockOutRecord(SecondComputer.ipAddress, SecondComputer.computerName, updatedTime);

        CheckRecordState(SecondComputer.ipAddress, SecondComputer.computerName, updatedTime,
            AttendanceStatusEnum.Normal);
    }

    [TestMethod]
    public void Given_EarlyClockOutTime_When_UpdateClockOutRecordWithValidTime_Then_RecordIsNormal()
    {
        var initialTime = new DateTime(2023, 7, 28, 18, 0, 00); // Early clock out time
        var updatedTime = new DateTime(2023, 7, 28, 19, 00, 00); // Valid clock out time

        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockOut, initialTime);
        AttendanceStatusShouldBe(AttendanceStatusEnum.Abnormal, AttendanceRecord.EarlyClockOutReason);

        WhenUpdateClockOutRecord(SecondComputer.ipAddress, SecondComputer.computerName, updatedTime);

        CheckRecordState(SecondComputer.ipAddress, SecondComputer.computerName, updatedTime,
            AttendanceStatusEnum.Normal);
    }

    [TestMethod]
    public void Given_ClockInType_When_UpdateClockOutRecord_Then_RecordIsNotUpdated()
    {
        var initialTime = new DateTime(2023, 7, 28, 9, 00, 00);
        var updatedTime = new DateTime(2023, 7, 28, 9, 30, 00);

        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockIn, initialTime);
        AttendanceStatusShouldBe(AttendanceStatusEnum.Normal);

        WhenUpdateClockOutRecord(SecondComputer.ipAddress, SecondComputer.computerName, updatedTime);

        CheckRecordState(FirstComputer.ipAddress, FirstComputer.computerName, initialTime, AttendanceStatusEnum.Normal);
    }
    
    [TestMethod]
    public void Given_NoClockInTime_When_CreateAttendanceRecord_Then_RecordShouldBeMarkedAsAbnormal()
    {
        GivenCreateAttendanceRecord(AttendanceTypeEnum.ClockIn, null);
        AttendanceStatusShouldBe(AttendanceStatusEnum.Abnormal, AttendanceRecord.NoClockInTimeReason);
    }

    private static void GivenCreateAttendanceRecord(AttendanceTypeEnum attendanceTypeEnum, DateTime? clockTime)
    {
        _attendanceRecord = AttendanceRecord.Create(
            EmployeeNumber,
            attendanceTypeEnum,
            FirstComputer.ipAddress,
            FirstComputer.computerName,
            clockTime);
    }

    private static void WhenUpdateClockOutRecord(string ipAddress, string computerName, DateTime clockTime)
    {
        _attendanceRecord.UpdateClockOutRecord(ipAddress, computerName, clockTime);
    }

    private static void AttendanceStatusShouldBe(AttendanceStatusEnum status, string reason = "")
    {
        _attendanceRecord.AttendanceStatus.Should().Be(new AttendanceStatus(status, reason,
            MangerProcessedTypeEnum.UnProcessed));
    }

    private static void CheckRecordState(string expectedIpAddress, string expectedComputerName,
        DateTime expectedClockTime, AttendanceStatusEnum expectedStatus)
    {
        _attendanceRecord.IpAddress.Should().Be(expectedIpAddress);
        _attendanceRecord.ComputerName.Should().Be(expectedComputerName);
        _attendanceRecord.ClockTime.Should().Be(expectedClockTime);
        AttendanceStatusShouldBe(expectedStatus);
    }
}