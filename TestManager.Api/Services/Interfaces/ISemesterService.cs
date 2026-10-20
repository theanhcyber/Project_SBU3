using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface ISemesterService
{
    Task<List<SemesterDto>> GetAllAsync();
    Task<SemesterDto?> GetByIdAsync(long id);
    Task<SemesterDto> CreateAsync(SemesterDto dto);
    Task<SemesterDto?> UpdateAsync(long id, SemesterDto dto);
    Task<bool> DeleteAsync(long id);
}
