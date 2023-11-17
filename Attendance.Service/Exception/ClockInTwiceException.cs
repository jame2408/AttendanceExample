namespace Attendance.Service.Exception;

public class ClockInTwiceException : InvalidOperationException
{
    public ClockInTwiceException(string message) : base(message)
    {
    }
}