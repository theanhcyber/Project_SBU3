using TestManager.Api.DTOs;

namespace TestManager.Api.Services;

public interface IStudentClassService
{
    Task<List<StudentClassDto>> GetAllAsync(long? classId = null, long? studentId = null);
    /// <summary>Danh sách sinh viên của lớp, cache Redis 60 giây (key <c>class:{classId}:students</c>).</summary>
    Task<List<StudentClassDto>> GetByClassCachedAsync(long classId);
    Task<StudentClassDto?> GetByIdAsync(long id);
    Task<StudentClassDto> CreateAsync(StudentClassDto dto);
    Task<StudentClassDto?> UpdateAsync(long id, StudentClassDto dto);
    Task<bool> DeleteAsync(long id);
}
