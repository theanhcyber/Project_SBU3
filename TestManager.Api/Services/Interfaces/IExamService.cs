using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface IExamService
{
    Task<List<ExamDto>> GetAllAsync(long? subjectId = null, long? teacherId = null);
    Task<ExamDto?> GetByIdAsync(long id);
    Task<ExamDto> CreateAsync(ExamDto dto);
    Task<ExamDto?> UpdateAsync(long id, ExamDto dto);
    Task<bool> DeleteAsync(long id);

    Task<ExamDto?> GetPaperAsync(long id);
    Task<ExamDto?> AddQuestionAsync(long id, ExamQuestionDto dto);
    Task<bool> RemoveQuestionAsync(long id, long questionId);
}
