using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class QuestionMapping : Profile
{
    public QuestionMapping()
    {
        CreateMap<Question, QuestionDto>()
            .ForMember(d => d.Answers, o => o.MapFrom(s => s.Answers.OrderBy(a => a.Id)));
    }
}
