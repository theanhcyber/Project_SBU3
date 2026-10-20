using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface ITopicService
{
    Task<List<TopicDto>> GetAllAsync(long? subjectId = null);
    Task<TopicDto?> GetByIdAsync(long id);
    Task<TopicDto> CreateAsync(TopicDto dto);
    Task<TopicDto?> UpdateAsync(long id, TopicDto dto);
    Task<bool> DeleteAsync(long id);
}
