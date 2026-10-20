using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface ITeacherService
{
    Task<List<TeacherDto>> GetAllAsync();
    Task<TeacherDto?> GetByIdAsync(long id);
    Task<TeacherDto> CreateAsync(TeacherDto dto);
    Task<TeacherDto?> UpdateAsync(long id, TeacherDto dto);
    Task<bool> DeleteAsync(long id);
}
