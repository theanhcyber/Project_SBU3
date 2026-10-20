using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho StudentMapper bên Java
public class StudentMapping : Profile
{
    // Cần Include User
    public StudentMapping()
    {
        CreateMap<Student, StudentDto>();
    }
}
