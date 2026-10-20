using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho UserMapper bên Java
public class UserMapping : Profile
{
    public UserMapping()
    {
        // Không bao giờ map mật khẩu ra ngoài
        CreateMap<User, UserDto>()
            .ForMember(d => d.Password, o => o.Ignore())
            .ForMember(d => d.CurrentPassword, o => o.Ignore());
    }
}
