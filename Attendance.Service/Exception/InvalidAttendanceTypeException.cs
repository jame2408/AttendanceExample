namespace Attendance.Service.Exception;

public class InvalidAttendanceTypeException : ArgumentException
{
    public InvalidAttendanceTypeException(string message, string paramName) : base(message, paramName)
    {
    }
}