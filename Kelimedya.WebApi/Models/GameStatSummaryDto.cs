using System;

namespace Kelimedya.WebAPI.Models
{
    public class GameStatSummaryDto
    {
        public int GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public int PlayCount { get; set; }
        public double AverageScore { get; set; }
    }
}
