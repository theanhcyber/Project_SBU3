using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class StudentClassService(AppDbContext db, IMapper mapper, IDistributedCache cache)
    : BaseService<StudentClass, StudentClassDto>(db, mapper), IStudentClassService
{
    private static readonly DistributedCacheEntryOptions StudentsCacheOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

    private static string StudentsCacheKey(long classId) => $"class:{classId}:students";

    public async Task<List<StudentClassDto>> GetByClassCachedAsync(long classId)
    {
        var key = StudentsCacheKey(classId);
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<StudentClassDto>>(cached)!;

        var students = await GetAllAsync(classId: classId);
        await cache.SetStringAsync(key, JsonSerializer.Serialize(students), StudentsCacheOptions);
        return students;
    }

    public async Task<List<StudentClassDto>> GetAllAsync(long? classId = null, long? studentId = null)
    {
        var query = Set.AsQueryable();
        if (classId is not null) query = query.Where(sc => sc.ClassId == classId);
        if (studentId is not null) query = query.Where(sc => sc.StudentId == studentId);

        return await ToDtoListAsync(query.OrderBy(sc => sc.Id));
    }

    public Task<StudentClassDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<StudentClassDto> CreateAsync(StudentClassDto dto)
    {
        await ValidateAsync(dto, excludeId: null);

        var studentClass = new StudentClass { ClassId = dto.ClassId, StudentId = dto.StudentId };
        Set.Add(studentClass);
        await Db.SaveChangesAsync();
        return (await GetByIdAsync(studentClass.Id))!;
    }

    public async Task<StudentClassDto?> UpdateAsync(long id, StudentClassDto dto)
    {
        var studentClass = await Set.FindAsync(id);
        if (studentClass is null) return null;

        await ValidateAsync(dto, excludeId: id);
        studentClass.ClassId = dto.ClassId;
        studentClass.StudentId = dto.StudentId;
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    private async Task ValidateAsync(StudentClassDto dto, long? excludeId)
    {
        if (!await Db.Classes.AnyAsync(c => c.Id == dto.ClassId))
            throw AppException.NotExists("Class", dto.ClassId);
        if (!await Db.Students.AnyAsync(s => s.Id == dto.StudentId))
            throw AppException.NotExists("Student", dto.StudentId);
        if (await Set.AnyAsync(sc =>
                sc.ClassId == dto.ClassId && sc.StudentId == dto.StudentId && sc.Id != excludeId))
            throw AppException.Conflict("Sinh viên đã ở trong lớp này.");
    }
}
