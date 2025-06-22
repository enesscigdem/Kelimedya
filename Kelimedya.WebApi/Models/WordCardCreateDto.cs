using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Kelimedya.WebAPI.Models
{
    public class WordCardCreateDto
    {
        [Required]
        public int LessonId { get; set; }
        
        [Required(ErrorMessage = "Kelime alanı gereklidir.")]
        public string Word { get; set; }
        
        [Required(ErrorMessage = "Eş anlam alanı gereklidir.")]
        public string Synonym { get; set; }
        
        [Required(ErrorMessage = "Tanım alanı gereklidir.")]
        public string Definition { get; set; }
        
        public string? ExampleSentence { get; set; }
        
        public IFormFile? ImageFile { get; set; }

        public List<GameQuestionDto>? GameQuestions { get; set; }

        public List<WordCardQuizQuestionDto>? QuizQuestions { get; set; }
    }
}