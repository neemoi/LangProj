using Application.Mapping;
using Application.MappingProfile;
using Application.Services.Implementations;
using Application.Services.Implementations.Auth;
using Application.Services.Implementations.Auth.JWT;
using Application.Services.Implementations.Functions;
using Application.Services.Implementations.KidQuiz;
using Application.Services.Implementations.Lesson.IQuizServ;
using Application.Services.Implementations.Lesson.Lessons;
using Application.Services.Implementations.Lesson.Phrasees;
using Application.Services.Implementations.Lesson.Progress;
using Application.Services.Implementations.Lesson.Words;
using Application.Services.Implementations.Lessons;
using Application.Services.Implementations.MainQuestions;
using Application.Services.Implementations.Name;
using Application.Services.Implementations.Nouns;
using Application.Services.Implementations.Pronunciation;
using Application.Services.Interfaces.IRepository.Auth;
using Application.Services.Interfaces.IRepository.Functions;
using Application.Services.Interfaces.IRepository.KidQuiz;
using Application.Services.Interfaces.IRepository.Lesons;
using Application.Services.Interfaces.IRepository.Lessons;
using Application.Services.Interfaces.IRepository.MainQuestions;
using Application.Services.Interfaces.IRepository.Name;
using Application.Services.Interfaces.IRepository.Nouns;
using Application.Services.Interfaces.IRepository.Profile;
using Application.Services.Interfaces.IRepository.Pronunciation;
using Application.Services.Interfaces.IServices.Auth;
using Application.Services.Interfaces.IServices.Functions;
using Application.Services.Interfaces.IServices.KidQuiz;
using Application.Services.Interfaces.IServices.Lesons;
using Application.Services.Interfaces.IServices.MainQuestions;
using Application.Services.Interfaces.IServices.Name;
using Application.Services.Interfaces.IServices.Nouns;
using Application.Services.Interfaces.IServices.Profile;
using Application.Services.Interfaces.IServices.Pronunciation;
using Application.UnitOfWork;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistance.Repository.Auth;
using Persistance.Repository.Functions;
using Persistance.Repository.KidQuiz;
using Persistance.Repository.Lesons.Leson;
using Persistance.Repository.Lesons.Progress;
using Persistance.Repository.Lesons.QuizLeson;
using Persistance.Repository.Lesons.QuizQuestionRep;
using Persistance.Repository.Lesons.Words;
using Persistance.Repository.Lessons.Lesson;
using Persistance.Repository.Name;
using Persistance.Repository.Nouns;
using Persistance.Repository.Pronunciation;
using Persistance.Repository.Userfsf;
using Persistance.UnitOfWork;
using Persistence.Repository.MainQuestions;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<LanguageLearningDbContext>(options =>
            options.UseMySql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 28))
            ));

        builder.Services.AddIdentity<Users, IdentityRole>()
            .AddEntityFrameworkStores<LanguageLearningDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
            };
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder => builder
                    .AllowAnyOrigin() 
                    .AllowAnyMethod()  
                    .AllowAnyHeader()); 
        });

        builder.Services.AddSingleton<IAuthEmailService>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var smtpSettings = configuration.GetSection("Email:Smtp");
            return new AuthEmailService(
                smtpSettings["Host"],
                int.Parse(smtpSettings["Port"]),
                smtpSettings["Username"],
                smtpSettings["Password"]
            );
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddLogging();

        builder.Services.AddAutoMapper(typeof(MappingAuthProfile), typeof(MappingUserProfile), typeof(MappingLessonWordProfile),
            typeof(MappingLessonPhraseProfile), typeof(MappingUserProgressProfile), typeof(MappingNounsProfile),
            typeof(MappingFunctionWordProfile), typeof(MappingPronunciationMappingProfile), typeof(MappingMainQuestionProfile),
            typeof(MappingNameProfile), typeof(MappingKidLessonProfile), typeof(MappingKidWordCardProfile),
            typeof(MappingKidQuizQuestionProfile), typeof(MappingKidQuizAnswerProfile));

        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ILessonRepository, LessonRepository>();
        builder.Services.AddScoped<ILessonWordRepository, LessonWordRepository>();
        builder.Services.AddScoped<ILessonPhraseRepository, LessonPhraseRepository>();
        builder.Services.AddScoped<IQuizRepository, LessonQuizRepository>();
        builder.Services.AddScoped<IQuizQuestionRepository, LessonQuizQuestionRepository>();
        builder.Services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        builder.Services.AddScoped<IAlphabetLetterRepository, AlphabetLetterRepository>();
        builder.Services.AddScoped<INounWordRepository, NounWordRepository>();
        builder.Services.AddScoped<IFunctionWordRepository, FunctionWordRepository>();
        builder.Services.AddScoped<IPartOfSpeechRepository, PartOfSpeechRepository>();
        builder.Services.AddScoped<IPronunciationRepository, PronunciationRepository>();
        builder.Services.AddScoped<IMainQuestionRepository, MainQuestionRepository>();
        builder.Services.AddScoped<IMaleNameRepository, MaleNameRepository>();
        builder.Services.AddScoped<IFemaleNameRepository, FemaleNameRepository>();
        builder.Services.AddScoped<IEnglishNameRepository, EnglishNameRepository>();
        builder.Services.AddScoped<IKidLessonRepository, KidLessonRepository>();
        builder.Services.AddScoped<IKidWordCardRepository, KidWordCardRepository>();
        builder.Services.AddScoped<IKidQuizQuestionRepository, KidQuizQuestionRepository>();
        builder.Services.AddScoped<IKidQuizAnswerRepository, KidQuizAnswerRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ILessonService, LessonService>();
        builder.Services.AddScoped<ILessonPhraseService, LessonPhraseService>();
        builder.Services.AddScoped<IQuizService, LessonQuizService>();
        builder.Services.AddScoped<IQuizQuestionService, LessonQuizQuestionService>();
        builder.Services.AddScoped<IUserProgressService, UserProgressService>();
        builder.Services.AddScoped<IAlphabetLetterService, AlphabetLetterService>();
        builder.Services.AddScoped<INounWordService, NounWordService>();
        builder.Services.AddScoped<IFunctionWordService, FunctionWordService>();
        builder.Services.AddScoped<IPartOfSpeechService, PartOfSpeechService>();
        builder.Services.AddScoped<IPronunciationService, PronunciationService>();
        builder.Services.AddScoped<ILessonWordService, LessonWordService>();
        builder.Services.AddScoped<IMainQuestionService, MainQuestionService>();
        builder.Services.AddScoped<IMaleNameService, MaleNameService>();
        builder.Services.AddScoped<IFemaleNameService, FemaleNameService>();
        builder.Services.AddScoped<INameService, NameService>();
        builder.Services.AddScoped<IKidLessonService, KidLessonService>();
        builder.Services.AddScoped<IKidWordCardService, KidWordCardService>();
        builder.Services.AddScoped<IKidQuizQuestionService, KidQuizQuestionService>();
        builder.Services.AddScoped<IKidQuizAnswerService, KidQuizAnswerService>();
        builder.Services.AddScoped<KidLessonService>();

        builder.WebHost.UseUrls("http://*:8080");

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LanguageLearningDbContext>();
            db.Database.Migrate();

            await SeedData(scope.ServiceProvider);

            // Set UTF-8 encoding for proper character support
            await db.Database.ExecuteSqlRawAsync("SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;");

            // Insert parts of speech
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `PartOfSpeechs` (`Id`, `Name`) VALUES 
            (1,'Article');
        ");

            // Insert function words
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `FunctionWords` (`Id`, `Name`, `Translation`, `PartOfSpeechId`) VALUES 
            (1,'a','indefinite article (before consonant sounds)',1),
            (2,'an','indefinite article (before vowel sounds)',1),
            (3,'the','definite article',1);
        ");

            // Insert pronunciation categories
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `PronunciationCategories` (`Id`, `Name`) VALUES 
            (1,'Office'),
            (2,'School'),
            (3,'Computer');
        ");

            // Insert lessons
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `Lessons` (`Id`, `Title`, `Description`, `VideoUrl`, `CreatedAt`, `PdfUrl`) VALUES 
            (1,'Domestic Animals and Animals 1 - Articles a, an, the','Lesson description: This lesson introduces nouns and articles. Nouns name objects. All nouns in this lesson refer to animal names. Most nouns are preceded by articles - words that indicate whether the noun refers to a general concept or a specific object.','https://youtu.be/6nNUApAQA0M','2025-05-13 11:25:55.305046','https://winner.gfriend.com/content/pdf/en/EN_Lesson1.pdf');
        ");

            // Insert lesson words
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `LessonWords` (`Id`, `LessonId`, `Name`, `Translation`, `ImageUrl`, `Type`) VALUES 
            (1,1,'animal','animal','https://winner.gfriend.com/Content/media/images/lessons48/tmb/001_01.jpg','Keyword'),
            (2,1,'cat','cat','https://winner.gfriend.com/Content/media/images/lessons48/tmb/001_02.jpg','Keyword'),
            (3,1,'cow','cow','https://winner.gfriend.com/Content/media/images/lessons48/tmb/001_03.jpg','Keyword'),
            (4,1,'dog','dog','https://winner.gfriend.com/Content/media/images/lessons48/tmb/001_04.jpg','Keyword'),
            (5,1,'horse','horse','https://winner.gfriend.com/Content/media/images/lessons48/tmb/001_05.jpg','Keyword'),
            (6,1,'a','indefinite article','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Keyword'),
            (7,1,'an','indefinite article (before vowels)','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Keyword'),
            (8,1,'the','definite article','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Keyword');
        ");

            // Insert lesson phrases
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `LessonPhrases` (`Id`, `LessonId`, `PhraseText`, `Translation`, `ImageUrl`) VALUES 
            (1,1,'a dog','a dog',''),
            (2,1,'a horse','a horse',''),
            (3,1,'animals','animals',''),
            (4,1,'cats','cats',''),
            (5,1,'cows','cows',''),
            (6,1,'dogs','dogs',''),
            (7,1,'horses','horses',''),
            (8,1,'the animal','the animal',''),
            (9,1,'the animals','the animals',''),
            (10,1,'the cat','the cat',''),
            (11,1,'the cats','the cats',''),
            (12,1,'the cow','the cow',''),
            (13,1,'the cows','the cows',''),
            (14,1,'the dog','the dog',''),
            (15,1,'the dogs','the dogs',''),
            (16,1,'the horse','the horse',''),
            (17,1,'the horses','the horses','');
        ");

            // Insert word items for pronunciation
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `WordItems` (`Id`, `Name`, `ImagePath`, `CategoryId`) VALUES 
            (1,'computer','https://example.com/images/computer.jpg',3),
            (2,'printer','https://example.com/images/printer.jpg',3),
            (3,'keyboard','https://example.com/images/keyboard.jpg',3),
            (4,'teacher','https://example.com/images/teacher.jpg',2),
            (5,'student','https://example.com/images/student.jpg',2),
            (6,'desk','https://example.com/images/desk.jpg',1),
            (7,'meeting','https://example.com/images/meeting.jpg',1);
        ");

            // Insert quizzes
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `Quizzes` (`Id`, `LessonId`, `Type`, `CreatedAt`) VALUES 
            (1,1,'NOUNS','2025-05-13 11:43:39.062206'),
            (2,1,'GRAMMAR','2025-05-13 11:47:42.676655');
        ");

            // Insert quiz questions
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `QuizQuestions` (`Id`, `QuizId`, `QuestionType`, `QuestionText`, `ImageUrl`, `AudioUrl`, `CorrectAnswer`) VALUES 
            (1,1,'ImageChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg',NULL,'Test'),
            (2,1,'ImageChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','','Test'),
            (3,1,'AudioChoice','Test',NULL,'http://localhost:3000/admin/lessons/1','Test'),
            (4,1,'AudioChoice','Test','','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Test'),
            (5,1,'ImageAudioChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Test 2'),
            (6,1,'ImageAudioChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Test 2'),
            (7,1,'Spelling','Test',NULL,NULL,'Test'),
            (8,1,'Spelling','Test','','','Test'),
            (9,1,'GrammarSelection','Test',NULL,NULL,'Test'),
            (10,1,'GrammarSelection','Test','','','Test'),
            (11,1,'Pronunciation','Test',NULL,NULL,'Test'),
            (12,1,'Pronunciation','Test','','','Test'),
            (13,1,'AdvancedSurvey','Test',NULL,NULL,'Test'),
            (14,1,'AdvancedSurvey','Test','','','Test'),
            (15,2,'ImageChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg',NULL,'Test'),
            (16,2,'ImageChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','','Test'),
            (17,2,'AudioChoice','Test',NULL,'https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Test'),
            (18,2,'AudioChoice','Test','','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Test'),
            (19,2,'ImageAudioChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Test'),
            (20,2,'ImageAudioChoice','Test','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg','Test'),
            (21,2,'Spelling','Test',NULL,NULL,'Test'),
            (22,2,'Spelling','Test','','','Test'),
            (23,2,'GrammarSelection','Test',NULL,NULL,'Test'),
            (24,2,'GrammarSelection','Test','','','Test'),
            (25,2,'GrammarSelection','Test',NULL,NULL,'Test'),
            (27,2,'Pronunciation','Test',NULL,NULL,'Test'),
            (28,2,'Pronunciation','Test','','','Test'),
            (29,2,'AdvancedSurvey','Test',NULL,NULL,'Test'),
            (30,2,'AdvancedSurvey','Test','','','Test');
        ");

            // Insert quiz answers
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `QuizAnswers` (`Id`, `QuestionId`, `AnswerText`, `IsCorrect`) VALUES 
            (1,1,'Test 1',0),
            (2,1,'Test 2',1),
            (3,1,'Test 3',0),
            (4,2,'Test 1',0),
            (5,2,'Test 2',1),
            (6,2,'Test 3',0),
            (7,3,'Test 1',0),
            (8,3,'Test 2',0),
            (9,3,'Test 3',0),
            (10,3,'Test',1),
            (15,4,'Test 1',0),
            (16,4,'Test 2',0),
            (17,4,'Test 3',0),
            (18,4,'Test',1),
            (19,5,'Test 1',0),
            (20,5,'Test 2',1),
            (21,6,'Test 1',0),
            (22,6,'Test 2',1),
            (23,7,'',0),
            (24,7,'',0),
            (25,8,'',0),
            (26,8,'',0),
            (27,9,'',0),
            (28,9,'',0),
            (29,10,'',0),
            (30,10,'',0),
            (31,11,'',0),
            (32,11,'',0),
            (33,12,'',0),
            (34,12,'',0),
            (35,13,'',0),
            (36,13,'',0),
            (37,14,'',0),
            (38,14,'',0),
            (39,15,'Test 1',0),
            (40,15,'Test 2',1),
            (41,16,'Test 1',0),
            (42,16,'Test 2',1),
            (43,17,'Test 1',0),
            (44,17,'Test 2',0),
            (45,17,'Test 3',1),
            (46,18,'Test 1',0),
            (47,18,'Test 2',0),
            (48,18,'Test 3',1),
            (49,19,'Test 1',0),
            (50,19,'Test 2',1),
            (51,20,'Test 1',0),
            (52,20,'Test 2',1),
            (53,21,'',0),
            (54,21,'',0),
            (55,22,'',0),
            (56,22,'',0),
            (57,23,'',0),
            (58,23,'',0),
            (59,24,'',0),
            (60,24,'',0),
            (61,25,'',0),
            (62,25,'',0),
            (65,27,'',0),
            (66,27,'',0),
            (67,28,'',0),
            (68,28,'',0),
            (69,29,'',0),
            (70,29,'',0),
            (71,30,'',0),
            (72,30,'',0);
        ");

            // Insert alphabet letters
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `AlphabetLetters` (`Id`, `Symbol`, `ImageUrl`) VALUES 
            (1,'A','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg'),
            (2,'B','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg'),
            (3,'C','https://winner.gfriend.com/Content/media/images/lessons48/tmb/aart.jpg');
        ");

            // Insert noun words
            await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO `NounWords` (`Id`, `Name`, `ImageUrl`, `AlphabetLetterId`) VALUES 
            (1,'accident','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0010.jpg',1),
            (2,'account','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0020.jpg',1),
            (3,'ache','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0030.jpg',1),
            (4,'baby','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0240.jpg',2),
            (5,'back','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0250.jpg',2),
            (6,'bag','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0260.jpg',2),
            (7,'cake','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0700.jpg',3),
            (8,'camera','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0710.jpg',3),
            (9,'car','https://winner.gfriend.com/Content/media/images/Noun/tmb/_0720.jpg',3);
        ");
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static async Task SeedData(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<Users>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        await SeedRoles(roleManager, logger);
        await SeedAdminUser(userManager, logger);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    logger.LogInformation($"Role '{roleName}' created successfully.");
                }
                else
                {
                    logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    private static async Task SeedAdminUser(UserManager<Users> userManager, ILogger logger)
    {
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            adminUser = new Users
            {
                UserName = "admin",
                Email = "admin@example.com"
            };
            var createUserResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createUserResult.Succeeded)
            {
                logger.LogInformation("Admin user created successfully.");
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (addRoleResult.Succeeded)
                {
                    logger.LogInformation("Admin role assigned to the user.");
                }
                else
                {
                    logger.LogError($"Failed to assign Admin role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogError($"Failed to create admin user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
