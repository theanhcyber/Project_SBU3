using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface IClassRoomService
{
    Task<List<ClassRoomDto>> GetAllAsync(long? semesterId = null, long? subjectId = null, long? teacherId = null);
    Task<ClassRoomDto?> GetByIdAsync(long id);
    Task<ClassRoomDto> CreateAsync(ClassRoomDto dto);
    Task<ClassRoomDto?> UpdateAsync(long id, ClassRoomDto dto);
    Task<bool> DeleteAsync(long id);
}
