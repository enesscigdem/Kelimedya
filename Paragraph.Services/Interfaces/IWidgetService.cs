namespace Paragraph.Services.Interfaces;

public interface IWidgetService
{
    Task<string> GetWidgetContentAsync(string key);

}