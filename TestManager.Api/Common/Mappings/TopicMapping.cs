using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho TopicMapper bên Java
public class TopicMapping : Profile
{
    // Cần Include Subject, Teacher, Questions. TeacherName được tự flatten từ Teacher.Name
    public TopicMapping()
    {
        CreateMap<Topic, TopicDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject.SubjectName))
            .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.Questions.Count));
    }
}
