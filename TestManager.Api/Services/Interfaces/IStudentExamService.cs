using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface IStudentExamService
{
    Task<List<StudentExamDto>> GetAllAsync(long? studentId = null, long? examId = null, bool? isTaken = null);
    Task<StudentExamDto?> GetByIdAsync(long id);
    Task<StudentExamDto> RegisterAsync(StudentExamDto dto);
    Task<List<StudentExamDto>> RegisterManyAsync(List<StudentExamDto> dtos);
    Task<StudentExamDto?> UpdateAsync(long id, StudentExamDto dto);
    Task<bool> DeleteAsync(long id);
    Task<StudentExamDto?> SubmitAsync(long id, StudentAnswerDto dto);
    Task<StudentExamDto?> MarkAsync(long id);
}
