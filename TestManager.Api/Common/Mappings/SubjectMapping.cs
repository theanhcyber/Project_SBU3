using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class SubjectMapping : Profile
{
    public SubjectMapping()
    {
        CreateMap<Subject, SubjectDto>();
    }
}
