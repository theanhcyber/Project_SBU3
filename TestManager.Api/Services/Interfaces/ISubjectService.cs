using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface ISubjectService
{
    Task<List<SubjectDto>> GetAllAsync();
    Task<SubjectDto?> GetByIdAsync(long id);
    Task<SubjectDto> CreateAsync(SubjectDto dto);
    Task<SubjectDto?> UpdateAsync(long id, SubjectDto dto);
    Task<bool> DeleteAsync(long id);
}
