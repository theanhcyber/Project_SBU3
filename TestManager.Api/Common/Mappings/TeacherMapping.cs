using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class TeacherMapping : Profile
{
    public TeacherMapping()
    {
        CreateMap<Teacher, TeacherDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject != null ? s.Subject.SubjectName : null));
    }
}
