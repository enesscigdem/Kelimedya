using System.Text.Json.Serialization;

namespace Kelimedya.WebAPI.Models;

public class RecordGameStatisticDto
{
    public string StudentId { get; set; } = string.Empty;
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int GameId { get; set; }
    public int Score { get; set; }
    public double DurationSeconds { get; set; }
}
