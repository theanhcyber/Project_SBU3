using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class StudentExamMapping : Profile
{
    public StudentExamMapping()
    {
        CreateMap<StudentExam, StudentExamDto>();
    }
}
