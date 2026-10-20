using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho QuestionMapper bên Java
public class QuestionMapping : Profile
{
    // Cần Include Topic, Answers. TopicCode được tự flatten từ Topic.Code
    public QuestionMapping()
    {
        CreateMap<Question, QuestionDto>()
            .ForMember(d => d.Answers, o => o.MapFrom(s => s.Answers.OrderBy(a => a.Id)));
    }
}
