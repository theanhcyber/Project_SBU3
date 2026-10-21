using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class ClassRoomMapping : Profile
{
    public ClassRoomMapping()
    {
        CreateMap<ClassRoom, ClassRoomDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject.SubjectName))
            .ForMember(d => d.StudentCount, o => o.MapFrom(s => s.StudentClasses.Count));
    }
}
