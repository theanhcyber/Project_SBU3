using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Password, o => o.Ignore())
            .ForMember(d => d.CurrentPassword, o => o.Ignore());
    }
}
