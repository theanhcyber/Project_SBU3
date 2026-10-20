using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(long id);
    Task<bool> ChangePasswordAsync(long id, UserDto dto);

    // Dùng chung cho Manager / Teacher / Student khi tạo, sửa tài khoản đi kèm
    Task<User> CreateUserAsync(UserDto dto);
    Task UpdateProfileAsync(User user, UserDto dto);
}
