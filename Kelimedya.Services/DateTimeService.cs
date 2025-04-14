using Kelimedya.Core.Interfaces.Business;

namespace Kelimedya.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now()
    {
        return DateTime.Now;
    }
    public DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }
}