using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho SubjectMapper bên Java
public class SubjectMapping : Profile
{
    public SubjectMapping()
    {
        CreateMap<Subject, SubjectDto>();
    }
}
