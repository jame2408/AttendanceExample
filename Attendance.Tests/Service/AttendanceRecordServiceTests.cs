using Attendance.Domain;
using Attendance.Service;
using Attendance.Service.DataContracts.Messages;
using Attendance.Service.Exception;
using Attendance.Service.Interface.Repository;

namespace Attendance.Tests.Service;

[TestClass]
public class AttendanceRecordServiceTests
{
    private IAttendanceRecordRepository _repository = null!;
    private AttendanceRecordService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = Substitute.For<IAttendanceRecordRepository>();
        _service = new AttendanceRecordService(_repository);
    }

    [TestMethod]
    public async Task ClockIn_NormalClockIn_RecordIsCreated()
    {
        var clockTime = new DateTime(2023, 7, 28, 08, 30, 00);
        var message =
            new AttendanceClockMessage("388496", AttendanceTypeEnum.ClockIn, "192.168.0.1", "NM388496", clockTime);

        _repository.GetByEmployeeNumberAndDateAsync(Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(new List<AttendanceRecord>());

        var result = await _service.ClockAsync(message);

        result.Type.Should().Be(AttendanceTypeEnum.ClockIn);
        result.ClockTime.Should().Be(clockTime);

        await _repository.Received(1).SaveAsync(Arg.Any<AttendanceRecord>());
    }

    [TestMethod]
    public async Task ClockIn_ExistingRecordWithoutTime_AllowsClockIn()
    {
        var clockTime = new DateTime(2023, 7, 28, 08, 30, 00);
        var message =
            new AttendanceClockMessage("388496", AttendanceTypeEnum.ClockIn, "192.168.0.1", "NM388496", clockTime);
        var existingRecord =
            AttendanceRecord.Create("388496", AttendanceTypeEnum.ClockIn, "192.168.0.1", "NM388496", null);

        _repository.GetByEmployeeNumberAndDateAsync(Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(new List<AttendanceRecord> { existingRecord });

        var result = await _service.ClockAsync(message);

        result.Type.Should().Be(AttendanceTypeEnum.ClockIn);
        result.ClockTime.Should().Be(clockTime);
        
        await _repository.Received(1).SaveAsync(Arg.Any<AttendanceRecord>());
    }
    
    [TestMethod]
    public async Task ClockIn_DoubleClockIn_ThrowsException()
    {
        var clockTime = new DateTime(2023, 7, 28, 08, 30, 00);
        var message =
            new AttendanceClockMessage("388496", AttendanceTypeEnum.ClockIn, "192.168.0.1", "NM388496", clockTime);
        var existingRecord =
            AttendanceRecord.Create("388496", AttendanceTypeEnum.ClockIn, "192.168.0.1", "NM388496", clockTime);

        _repository.GetByEmployeeNumberAndDateAsync(Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(new List<AttendanceRecord> { existingRecord });

        var act = async () => await _service.ClockAsync(message);

        await act.Should().ThrowAsync<ClockInTwiceException>()
            .WithMessage("Cannot clock in twice in the same day.");
    }

    [TestMethod]
    public async Task Given_NoExistingClockInRecord_When_ClockOut_Then_AbnormalClockInRecordShouldBeCreated()
    {
        var message = new AttendanceClockMessage("388496", AttendanceTypeEnum.ClockOut, "192.168.0.2", "NM421331",
            DateTime.Now);
        _repository.GetByEmployeeNumberAndDateAsync(message.EmployeeNumber, message.ClockTime.Date)
            .Returns(new List<AttendanceRecord>());

        var result = await _service.ClockAsync(message);

        await _repository.Received(1).SaveAsync(Arg.Is<List<AttendanceRecord>>(records =>
            records.Any(record => record.Type == AttendanceTypeEnum.ClockIn && record.ClockTime == null)));
        result.Type.Should().Be(AttendanceTypeEnum.ClockOut);
    }

    [TestMethod]
    public async Task Given_ExistingClockOutRecord_When_ClockOut_Then_ClockOutRecordShouldBeUpdated()
    {
        var message = new AttendanceClockMessage("388496", AttendanceTypeEnum.ClockOut, "192.168.0.2", "NM421331",
            DateTime.Now);
        var existingClockOutRecord = AttendanceRecord.Create(message.EmployeeNumber, AttendanceTypeEnum.ClockOut,
            message.IpAddress, message.ComputerName, message.ClockTime.AddHours(-1));
        _repository.GetByEmployeeNumberAndDateAsync(message.EmployeeNumber, message.ClockTime.Date)
            .Returns(new List<AttendanceRecord> { existingClockOutRecord });

        var result = await _service.ClockAsync(message);

        await _repository.Received(1).SaveAsync(Arg.Is<List<AttendanceRecord>>(records =>
            records.Any(record =>
                record.Type == AttendanceTypeEnum.ClockOut && record.ClockTime == message.ClockTime)));
        result.Type.Should().Be(AttendanceTypeEnum.ClockOut);
        result.ClockTime.Should().Be(message.ClockTime);
    }

    [TestMethod]
    public async Task Given_NoExistingClockOutRecord_When_ClockOut_Then_ClockOutRecordShouldBeCreated()
    {
        var message = new AttendanceClockMessage("388496", AttendanceTypeEnum.ClockOut, "192.168.0.2", "NM421331",
            DateTime.Now);
        var existingClockInRecord = AttendanceRecord.Create(message.EmployeeNumber, AttendanceTypeEnum.ClockIn,
            message.IpAddress, message.ComputerName, message.ClockTime.AddHours(-1));
        _repository.GetByEmployeeNumberAndDateAsync(message.EmployeeNumber, message.ClockTime.Date)
            .Returns(new List<AttendanceRecord> { existingClockInRecord });

        var result = await _service.ClockAsync(message);

        await _repository.Received(1).SaveAsync(Arg.Is<List<AttendanceRecord>>(records =>
            records.Any(record =>
                record.Type == AttendanceTypeEnum.ClockOut && record.ClockTime == message.ClockTime)));
        result.Type.Should().Be(AttendanceTypeEnum.ClockOut);
        result.ClockTime.Should().Be(message.ClockTime);
    }
}