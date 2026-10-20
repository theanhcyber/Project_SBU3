using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho StudentClassMapper bên Java
public class StudentClassMapping : Profile
{
    // Cần Include ClassRoom, Student. StudentName được tự flatten từ Student.Name
    public StudentClassMapping()
    {
        CreateMap<StudentClass, StudentClassDto>()
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.ClassRoom.Name))
            .ForMember(d => d.ClassCode, o => o.MapFrom(s => s.ClassRoom.Code))
            .ForMember(d => d.StudentCode, o => o.MapFrom(s => s.Student.StudentCode));
    }
}
