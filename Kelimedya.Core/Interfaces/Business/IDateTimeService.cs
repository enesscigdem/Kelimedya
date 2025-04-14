namespace Kelimedya.Core.Interfaces.Business
{
    public interface IDateTimeService
    {
        DateTime Now();
        DateTime UtcNow();
    }
}