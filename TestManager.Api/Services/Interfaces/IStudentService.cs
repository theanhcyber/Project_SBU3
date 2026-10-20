using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface IStudentService
{
    Task<List<StudentDto>> GetAllAsync();
    Task<StudentDto?> GetByIdAsync(long id);
    Task<StudentDto> CreateAsync(StudentDto dto);
    Task<StudentDto?> UpdateAsync(long id, StudentDto dto);
    Task<bool> DeleteAsync(long id);
}
