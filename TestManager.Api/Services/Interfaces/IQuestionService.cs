using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface IQuestionService
{
    Task<List<QuestionDto>> GetAllAsync(long? topicId = null);
    Task<QuestionDto?> GetByIdAsync(long id);
    Task<QuestionDto> CreateAsync(QuestionDto dto);
    Task<QuestionDto?> UpdateAsync(long id, QuestionDto dto);
    Task<bool> DeleteAsync(long id);
    Task<List<QuestionDto>> ImportExcelAsync(Stream excel);
    byte[] BuildExcelTemplate();
}
