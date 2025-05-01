﻿using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class QuizQuestion
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public int QuizId { get; set; }
        
        [Required]
        public string? QuestionType { get; set; } //Question type ('image_choice', 'audio_choice',
                                                  //'image_audio_choice', 'spelling', 'grammar selection',
                                                  //'pronunciation', 'advanced survey' )

        public string? QuestionText { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? AudioUrl { get; set; }
        
        public string? CorrectAnswer { get; set; }


        public Quiz? Quiz { get; set; }
        public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}
