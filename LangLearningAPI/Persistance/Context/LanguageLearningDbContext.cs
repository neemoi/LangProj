﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Infrastructure.Data
{
    public class LanguageLearningDbContext : IdentityDbContext<Users>
    {
        public LanguageLearningDbContext(DbContextOptions<LanguageLearningDbContext> options)
            : base(options) { }

        public new DbSet<Users>? Users { get; set; }
        public DbSet<Lesson>? Lessons { get; set; }
        public DbSet<LessonWord>? LessonWords { get; set; }
        public DbSet<LessonPhrase>? LessonPhrases { get; set; }
        public DbSet<Quiz>? Quizzes { get; set; }
        public DbSet<QuizQuestion>? QuizQuestions { get; set; }
        public DbSet<QuizAnswer>? QuizAnswers { get; set; }
        public DbSet<UserProgress>? UserProgress { get; set; }
        public DbSet<UserWordProgress>? UserWordProgress { get; set; }
        public DbSet<AlphabetLetter> AlphabetLetters { get; set; }
        public DbSet<NounWord> NounWords { get; set; }
        public DbSet<PartOfSpeech> PartOfSpeechs { get; set; }
        public DbSet<FunctionWord> FunctionWords { get; set; }
        public DbSet<WordItem> WordItems { get; set; }
        public DbSet<PronunciationCategory> PronunciationCategories{ get; set; }
        public DbSet<MainQuestionWord> MainQuestionWords{ get; set; }
        public DbSet<MainQuestion> MainQuestions{ get; set; }
        public DbSet<EnglishName> EnglishNames { get; set; }
        public DbSet<FemaleName> FemaleNames { get; set; }
        public DbSet<MaleName> MaleNames { get; set; }
        public DbSet<KidLesson> KidLessons { get; set; }
        public DbSet<KidWordCard> KidWordCards { get; set; }
        public DbSet<KidQuizType> KidQuizTypes { get; set; }
        public DbSet<KidQuizQuestion> KidQuizQuestions { get; set; }
        public DbSet<KidQuizAnswer> KidQuizAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LessonWord>(entity =>
            {
                entity.HasOne(lw => lw.Lesson)
                      .WithMany(l => l.Words)
                      .HasForeignKey(lw => lw.LessonId);
            });

            modelBuilder.Entity<LessonPhrase>(entity =>
            {
                entity.HasOne(lp => lp.Lesson)
                      .WithMany(l => l.Phrases)
                      .HasForeignKey(lp => lp.LessonId);
            });

            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasOne(q => q.Lesson)
                      .WithMany(l => l.Quizzes)
                      .HasForeignKey(q => q.LessonId);
            });

            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.HasOne(qq => qq.Quiz)
                      .WithMany(q => q.Questions)
                      .HasForeignKey(qq => qq.QuizId);
            });

            modelBuilder.Entity<QuizAnswer>(entity =>
            {
                entity.HasOne(qa => qa.Question)
                      .WithMany(qq => qq.Answers)
                      .HasForeignKey(qa => qa.QuestionId);
            });

            modelBuilder.Entity<UserProgress>(entity =>
            {
                entity.HasOne(up => up.User)
                      .WithMany()
                      .HasForeignKey(up => up.UserId);

                entity.HasOne(up => up.Lesson)
                      .WithMany()
                      .HasForeignKey(up => up.LessonId);

                entity.HasOne(up => up.Quiz)
                      .WithMany()
                      .HasForeignKey(up => up.QuizId);
            });

            modelBuilder.Entity<UserWordProgress>(entity =>
            {
                entity.HasOne(uwp => uwp.User)
                      .WithMany()
                      .HasForeignKey(uwp => uwp.UserId);

                entity.HasOne(uwp => uwp.Lesson)
                      .WithMany()
                      .HasForeignKey(uwp => uwp.LessonId);

                entity.HasOne(uwp => uwp.Word)
                      .WithMany()
                      .HasForeignKey(uwp => uwp.WordId);
            });

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasIndex(l => l.Title);
                entity.HasIndex(l => l.PdfUrl);
            });

            modelBuilder.Entity<KidQuizType>().HasData(
                new KidQuizType { Id = 1, Name = "image_choice" },
                new KidQuizType { Id = 2, Name = "audio_choice" },
                new KidQuizType { Id = 3, Name = "image_audio_choice" },
                new KidQuizType { Id = 4, Name = "spelling" }
            );

            modelBuilder.Entity<Quiz>()
                .HasIndex(q => q.LessonId);

            modelBuilder.Entity<QuizQuestion>()
                .HasIndex(qq => qq.QuizId);

            modelBuilder.Entity<QuizAnswer>()
                .HasIndex(qa => qa.QuestionId);

            modelBuilder.Entity<UserProgress>()
                .HasIndex(up => up.UserId);

            modelBuilder.Entity<UserProgress>()
                .HasIndex(up => up.LessonId);

            modelBuilder.Entity<UserProgress>()
                .HasIndex(up => up.QuizId);

            modelBuilder.Entity<UserWordProgress>()
                .HasIndex(uwp => uwp.UserId);

            modelBuilder.Entity<UserWordProgress>()
                .HasIndex(uwp => uwp.LessonId);

            modelBuilder.Entity<UserWordProgress>()
                .HasIndex(uwp => uwp.WordId);

            modelBuilder.Entity<UserProgress>()
                .HasIndex(up => new { up.UserId, up.LessonId });

            modelBuilder.Entity<UserWordProgress>()
                .HasIndex(uwp => new { uwp.UserId, uwp.WordId });
        }
    }
}