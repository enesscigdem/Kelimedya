namespace Paragraph.Core.Interfaces.Business
{
    public interface IDateTimeService
    {
        DateTime Now();

        DateTime UtcNow();
    }
}