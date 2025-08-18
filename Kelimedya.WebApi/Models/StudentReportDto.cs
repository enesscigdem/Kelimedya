using System;
using System.Collections.Generic;

namespace Kelimedya.WebAPI.Models
{
    // StudentReports listesi için
    public class StudentReportDto
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public List<LessonReportDto> CompletedLessons { get; set; } = new();
        public List<WordReportDto> LearnedWords { get; set; } = new();
    }

    public class WordReportDto
    {
        public int WordCardId { get; set; }
        public string Word { get; set; }
        public string Synonym { get; set; }
        public string Definition { get; set; }
        public string ExampleSentence { get; set; }
        public int ViewCount { get; set; }
    }

    // KPI özet
    public class TeacherOverviewDto
    {
        public int TotalStudents { get; set; }
        public double AvgLessonCompletion { get; set; }
        public int TotalLearnedWords { get; set; }
        public int TotalQuizAttempts { get; set; }
        public double AvgQuizScore { get; set; }
        public int TotalGamePoints { get; set; }
    }

    // Karne
    public class TranscriptDto
    {
        public string HtmlContent { get; set; }
        public string PdfDownloadUrl { get; set; }
        public string StudentName { get; set; }
        public int TotalQuizAttempts { get; set; }
        public double AvgQuizScore { get; set; }
        public int TotalGamePoints { get; set; }
        public List<QuizAttemptItemDto> LastQuizAttempts { get; set; } = new();
    }

    public class QuizAttemptItemDto
    {
        public DateTime Date { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Score { get; set; }
    }

    // Detaylı
    public class DetailedReportDto
    {
        public string HtmlContent { get; set; }
        public string PdfDownloadUrl { get; set; }
        public string StudentName { get; set; }
        public int CompletedLessonCount { get; set; }
        public int LearnedWordCount { get; set; }
        public double AvgLessonCompletion { get; set; }
        public int TotalQuizAttempts { get; set; }
        public double AvgQuizScore { get; set; }
        public int TotalGamePoints { get; set; }
        public List<QuizAttemptItemDto> LastQuizAttempts { get; set; } = new();
        public List<GamePointItemDto> LastGamePoints { get; set; } = new();
        public List<WordStatsDto> TopWords { get; set; } = new(); // eklendi
        public List<WrongAnswerDto> WrongAnswers { get; set; } = new(); // eklendi
    }

    public class WordStatsDto
    {
        public int WordCardId { get; set; }
        public string Word { get; set; } = "";
        public string Definition { get; set; } = "";
        public int ViewCount { get; set; }
        public double TimeSpentSeconds { get; set; }           // StudentWordCardProgress.ResponseTimeTotalSeconds
        public double AvgResponseTimeSeconds { get; set; }     // StudentWordCardProgress.AverageResponseTimeSeconds
        public DateTime? LastSeenDate { get; set; }            // StudentWordCardProgress.LastSeenDate
    }

    public class WrongAnswerDto
    {
        public DateTime Date { get; set; }
        public string? LessonTitle { get; set; }
        public string? Question { get; set; }
        public string? GivenAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
    }

    public class GamePointItemDto
    {
        public DateTime Date { get; set; }
        public string GameName { get; set; }
        public int Score { get; set; }
    }
}