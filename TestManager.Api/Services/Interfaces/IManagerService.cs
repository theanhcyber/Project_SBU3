using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface IManagerService
{
    Task<List<ManagerDto>> GetAllAsync();
    Task<ManagerDto?> GetByIdAsync(long id);
    Task<ManagerDto> CreateAsync(ManagerDto dto);
    Task<ManagerDto?> UpdateAsync(long id, ManagerDto dto);
    Task<ManagerDto?> ToggleStatusAsync(long id);
    Task<bool> DeleteAsync(long id);
}
