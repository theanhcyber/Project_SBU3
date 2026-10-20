using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho TeacherMapper bên Java
public class TeacherMapping : Profile
{
    // Cần Include User, Subject
    public TeacherMapping()
    {
        CreateMap<Teacher, TeacherDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject != null ? s.Subject.SubjectName : null));
    }
}
