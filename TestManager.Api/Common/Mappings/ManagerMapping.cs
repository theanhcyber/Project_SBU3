using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class ManagerMapping : Profile
{
    // Cần Include User
    public ManagerMapping()
    {
        CreateMap<Manager, ManagerDto>();
    }
}
