using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho ClassMapper bên Java
public class ClassRoomMapping : Profile
{
    // Cần Include Subject, Semester, Teacher, StudentClasses.
    // SemesterName, TeacherName được AutoMapper tự flatten từ Semester.Name, Teacher.Name
    public ClassRoomMapping()
    {
        CreateMap<ClassRoom, ClassRoomDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject.SubjectName))
            .ForMember(d => d.StudentCount, o => o.MapFrom(s => s.StudentClasses.Count));
    }
}
