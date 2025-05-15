using Application.DtoModels.Lessons.QuizQuestion;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfile
{
    public class MappingQuizQuestionProfile : Profile
    {
        public MappingQuizQuestionProfile()
        {
            CreateMap<QuizQuestion, QuizQuestionDto>().ReverseMap();
            CreateMap<DtoModels.Lessons.Quiz.QuizAnswerDto, QuizAnswer>().ReverseMap();
            CreateMap<CreateQuizQuestionDto, QuizQuestion>();
            CreateMap<UpdateQuizQuestionDto, QuizQuestion>();
        }
    }
}
