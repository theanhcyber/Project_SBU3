using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class SemesterMapping : Profile
{
    public SemesterMapping()
    {
        CreateMap<Semester, SemesterDto>();
    }
}
