using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class StudentMapping : Profile
{
    public StudentMapping()
    {
        CreateMap<Student, StudentDto>();
    }
}
