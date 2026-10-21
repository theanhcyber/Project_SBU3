using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class TopicMapping : Profile
{
    public TopicMapping()
    {
        CreateMap<Topic, TopicDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject.SubjectName))
            .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.Questions.Count));
    }
}
