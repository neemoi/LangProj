using Application.DtoModels.Lessons.QuizQuestion;
using Application.Services.Interfaces.IRepository.Lesons;
using Common.Exceptions;
using Domain.Models;
using Infrastructure.Data;
using LangLearningAPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Persistance.Repository.Lesons.QuizQuestionRep
{
    public class LessonQuizQuestionRepository : IQuizQuestionRepository
    {
        private readonly LanguageLearningDbContext _context;
        private readonly ILogger<LessonQuizQuestionRepository> _logger;

        public LessonQuizQuestionRepository(LanguageLearningDbContext context, ILogger<LessonQuizQuestionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<QuizQuestion> CreateQuizQuestionAsync(QuizQuestion entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to create a quiz question with null entity.");
                throw new ArgumentNullException(nameof(entity), "Quiz question entity cannot be null");
            }

            if (string.IsNullOrWhiteSpace(entity.QuestionText))
            {
                _logger.LogWarning("Attempted to create a quiz question with empty question text.");
                throw new ArgumentException("Question text cannot be null or empty", nameof(entity.QuestionText));
            }

            var allowedQuestionTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ImageChoice",
                "AudioChoice",
                "ImageAudioChoice",
                "Spelling",
                "GrammarSelection",
                "Pronunciation",
                "AdvancedSurvey"
            };

            if (string.IsNullOrWhiteSpace(entity.QuestionType) ||
                !allowedQuestionTypes.Contains(entity.QuestionType))
            {
                _logger.LogWarning(
                    "Invalid question type: {QuestionType}. Allowed types: {AllowedTypes}",
                    entity.QuestionType,
                    string.Join(", ", allowedQuestionTypes));

                throw new ArgumentException(
                    $"Invalid question type. Allowed types: {string.Join(", ", allowedQuestionTypes)}",
                    nameof(entity.QuestionType));
            }

            switch (entity.QuestionType.ToLower())
            {
                case "image_choice":
                    if (string.IsNullOrEmpty(entity.ImageUrl))
                    {
                        throw new ArgumentException("Image URL is required for image_choice type",
                            nameof(entity.ImageUrl));
                    }
                    break;

                case "audio_choice":
                    if (string.IsNullOrEmpty(entity.AudioUrl))
                    {
                        throw new ArgumentException("Audio URL is required for audio_choice type",
                            nameof(entity.AudioUrl));
                    }
                    break;

                case "image_audio_choice":
                    if (string.IsNullOrEmpty(entity.ImageUrl) || string.IsNullOrEmpty(entity.AudioUrl))
                    {
                        throw new ArgumentException(
                            "Both Image URL and Audio URL are required for image_audio_choice type");
                    }
                    break;
            }

            try
            {
                await _context.QuizQuestions.AddAsync(entity);
                await _context.SaveChangesAsync();

                return entity;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while adding quiz question");
                throw new DatabaseException("An error occurred while adding the quiz question.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while adding quiz question");
                throw new ApplicationException("An unexpected error occurred while adding the quiz question.", ex);
            }
        }

        public async Task<QuizQuestion?> GetQuizQuestionByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Attempted to retrieve quiz question with invalid ID.");
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be greater than zero");
            }

            try
            {
                return await _context.QuizQuestions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving quiz question with ID {id}");
                throw new ApplicationException($"An unexpected error occurred while retrieving quiz question with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<QuizQuestion>> GetQuizQuestionAllAsync()
        {
            try
            {
                var questions = await _context.QuizQuestions
                    .Include(q => q.Answers)
                    .ToListAsync();

                if (questions == null || !questions.Any())
                {
                    _logger.LogWarning("No quiz questions found.");
                    throw new NotFoundException("No quiz questions found in the database.", "QUIZ_QUESTIONS_NOT_FOUND");
                }

                return questions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all quiz questions");
                throw new ApplicationException("An unexpected error occurred while retrieving all quiz questions.", ex);
            }
        }

        public async Task<QuizQuestion> UpdateQuizQuestionAsync(UpdateQuizQuestionDto dto)
        {
            var quizQuestion = await _context.QuizQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == dto.Id);

            if (quizQuestion == null)
                throw new NotFoundException($"QuizQuestion with ID {dto.Id} not found.");

            quizQuestion.QuestionType = dto.QuestionType ?? quizQuestion.QuestionType;
            quizQuestion.QuestionText = dto.QuestionText ?? quizQuestion.QuestionText;
            quizQuestion.ImageUrl = dto.ImageUrl ?? quizQuestion.ImageUrl;
            quizQuestion.AudioUrl = dto.AudioUrl ?? quizQuestion.AudioUrl;
            quizQuestion.CorrectAnswer = dto.CorrectAnswer ?? quizQuestion.CorrectAnswer;

            if (dto.Answers != null)
            {
                var incomingIds = dto.Answers
                    .Where(a => a.Id != 0)
                    .Select(a => a.Id)
                    .ToList();

                var answersToRemove = quizQuestion.Answers
                    .Where(a => !incomingIds.Contains(a.Id))
                    .ToList(); 

                foreach (var answer in answersToRemove)
                {
                    quizQuestion.Answers.Remove(answer);
                }

                foreach (var answerDto in dto.Answers)
                {
                    if (answerDto.Id != 0)
                    {
                        var existingAnswer = quizQuestion.Answers.FirstOrDefault(a => a.Id == answerDto.Id);
                        if (existingAnswer != null)
                        {
                            existingAnswer.AnswerText = answerDto.AnswerText;
                            existingAnswer.IsCorrect = answerDto.IsCorrect;
                        }
                    }
                    else
                    {
                        quizQuestion.Answers.Add(new QuizAnswer
                        {
                            AnswerText = answerDto.AnswerText,
                            IsCorrect = answerDto.IsCorrect
                        });
                    }
                }
            }

            _context.QuizQuestions.Update(quizQuestion);
            await _context.SaveChangesAsync();

            return quizQuestion;
        }


        public async Task<QuizQuestion> DeleteQuizQuestionAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Attempted to delete quiz question with invalid ID.");
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be greater than zero");
            }

            try
            {
                var existing = await _context.QuizQuestions.FindAsync(id)
                    ?? throw new NotFoundException($"QuizQuestion with ID {id} not found", "QUIZ_QUESTION_NOT_FOUND");

                _context.QuizQuestions.Remove(existing);
                await _context.SaveChangesAsync();
                
                return existing;
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "QuizQuestion not found for deletion");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting quiz question");
                throw new DatabaseException("An error occurred while deleting the quiz question.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting quiz question");
                throw new ApplicationException("An unexpected error occurred while deleting the quiz question.", ex);
            }
        }

        public async Task<QuizAnswer> DeleteQuizAnswerAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Attempted to delete quiz answer with invalid ID.");
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be greater than zero");
            }

            try
            {
                var answer = await _context.QuizAnswers.FindAsync(id);
                if (answer == null)
                {
                    _logger.LogWarning("Quiz answer with ID {id} not found.", id);
                    throw new NotFoundException($"QuizAnswer with ID {id} not found", "QUIZ_ANSWER_NOT_FOUND");
                }

                _context.QuizAnswers.Remove(answer);
                await _context.SaveChangesAsync();

                return answer;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting quiz answer");
                throw new DatabaseException("An error occurred while deleting the quiz answer.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting quiz answer");
                throw new ApplicationException("An unexpected error occurred while deleting the quiz answer.", ex);
            }
        }

        public async Task<bool> QuizExistsAsync(int quizId)
        {
            if (quizId <= 0)
            {
                _logger.LogWarning("Attempted to check existence of quiz with invalid ID.");
                throw new ArgumentOutOfRangeException(nameof(quizId), "Quiz ID must be greater than zero");
            }

            try
            {
                return await _context.Quizzes.AnyAsync(q => q.Id == quizId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while checking existence of quiz with ID {quizId}");
                throw new ApplicationException($"An unexpected error occurred while checking existence of quiz with ID {quizId}.", ex);
            }
        }

        public async Task<IEnumerable<QuizQuestion>> GetByQuizIdAsync(int quizId)
        {
            if (quizId <= 0)
            {
                _logger.LogWarning("Attempted to retrieve quiz questions with invalid quiz ID.");
                throw new ArgumentOutOfRangeException(nameof(quizId), "Quiz ID must be greater than zero");
            }

            try
            {
                var questions = await _context.QuizQuestions
                    .Where(q => q.QuizId == quizId)
                    .Include(q => q.Answers)
                    .ToListAsync();

                if (questions == null || !questions.Any())
                {
                    _logger.LogWarning($"No quiz questions found for quiz ID {quizId}");
                    throw new NotFoundException($"No quiz questions found for quiz ID {quizId}", "QUIZ_QUESTIONS_NOT_FOUND");
                }

                return questions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving quiz questions for quiz ID {quizId}");
                throw new ApplicationException($"An unexpected error occurred while retrieving quiz questions for quiz ID {quizId}.", ex);
            }
        }
    }
}
