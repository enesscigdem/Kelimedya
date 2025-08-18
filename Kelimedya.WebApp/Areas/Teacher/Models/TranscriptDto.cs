// Areas/Teacher/Models/TranscriptDto.cs
public class TranscriptDto
{
    public string HtmlContent { get; set; }          // geriye uyumluluk: HTML hazır içerik
    public string PdfDownloadUrl { get; set; }
    public string StudentName { get; set; }          // eklendi
    public int TotalQuizAttempts { get; set; }       // eklendi
    public double AvgQuizScore { get; set; }         // 0-100
    public int TotalGamePoints { get; set; }         // oyunlardan gelen toplam puan
    public List<QuizAttemptItemDto> LastQuizAttempts { get; set; } = new();
}

public class QuizAttemptItemDto
{
    public DateTime Date { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int Score { get; set; }
}

// Areas/Teacher/Models/DetailedReportDto.cs
public class DetailedReportDto
{
    public string HtmlContent { get; set; }
    public string PdfDownloadUrl { get; set; }
    public string StudentName { get; set; }           // eklendi

    // İstatistik blokları
    public int CompletedLessonCount { get; set; }
    public int LearnedWordCount { get; set; }
    public double AvgLessonCompletion { get; set; }   // 0-100
    public int TotalQuizAttempts { get; set; }
    public double AvgQuizScore { get; set; }
    public int TotalGamePoints { get; set; }

    // Opsiyonel: son 10 quiz, son 10 oyun
    public List<QuizAttemptItemDto> LastQuizAttempts { get; set; } = new();
    public List<GamePointItemDto> LastGamePoints { get; set; } = new();
}

public class GamePointItemDto
{
    public DateTime Date { get; set; }
    public string GameName { get; set; }
    public int Score { get; set; }
}