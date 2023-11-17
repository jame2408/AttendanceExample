using Attendance.Api.Extensions;
using Attendance.Api.Helpers;
using Attendance.Api.Models.Request;
using Attendance.Api.Models.Response;
using Attendance.Domain;
using Attendance.Service.DataContracts.Messages;
using Attendance.Service.Interface.Service;
using Microsoft.AspNetCore.Mvc;

namespace Attendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceRecordController : ControllerBase
{
    private readonly IAttendanceRecordService _attendanceRecordService;
    private readonly IValidationService _validator;

    public AttendanceRecordController(IAttendanceRecordService attendanceRecordService, IValidationService validator)
    {
        _attendanceRecordService = attendanceRecordService;
        _validator = validator;
    }

    /// <summary>
    /// 上/下班打卡紀錄
    /// </summary>
    /// <param name="request">提供打卡紀錄所需要的資訊</param>
    /// <returns></returns>
    [HttpPatch("clock", Name = "Clock")]
    public async Task<ActionResult<AttendanceClockResponse>> Clock([FromBody] AttendanceClockRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (validationResult.IsValid == false)
        {
            return BadRequest(validationResult.ToValidationProblemDetails(HttpContext.Request.Path));
        }

        var result = await _attendanceRecordService.ClockAsync(MapAttendanceClockRequestToMessage(request));
        return Ok(new AttendanceClockResponse(result));
    }

    private AttendanceClockMessage MapAttendanceClockRequestToMessage(AttendanceClockRequest request) =>
        new(request.EmployeeNumber,
            request.Type, request.IpAddress, request.ComputerName, request.ClockTime);
}