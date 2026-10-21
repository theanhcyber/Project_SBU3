using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class TeacherService(AppDbContext db, IMapper mapper, IUserService users) : BaseService<Teacher, TeacherDto>(db, mapper), ITeacherService
{
    public async Task<List<TeacherDto>> GetAllAsync() =>
        await ToDtoListAsync(Set.OrderBy(t => t.Id));

    public Task<TeacherDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<TeacherDto> CreateAsync(TeacherDto dto)
    {
        await EnsureSubjectExistsAsync(dto.SubjectId);

        var teacher = new Teacher
        {
            Name = dto.Name,
            Status = true,
            SubjectId = dto.SubjectId,
            User = await users.CreateUserAsync(dto.User)
        };
        Set.Add(teacher);
        await Db.SaveChangesAsync();
        return (await GetByIdAsync(teacher.Id))!;
    }

    public async Task<TeacherDto?> UpdateAsync(long id, TeacherDto dto)
    {
        var teacher = await Set.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
        if (teacher is null) return null;

        await EnsureSubjectExistsAsync(dto.SubjectId);
        await users.UpdateProfileAsync(teacher.User, dto.User);
        teacher.Name = dto.Name;
        teacher.Status = dto.Status ?? teacher.Status;
        teacher.SubjectId = dto.SubjectId;
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id) =>
        await Db.Users.Where(u => u.Teacher != null && u.Teacher.Id == id).ExecuteDeleteAsync() > 0;

    private async Task EnsureSubjectExistsAsync(long? subjectId)
    {
        if (subjectId is not null && !await Db.Subjects.AnyAsync(s => s.Id == subjectId))
            throw AppException.NotExists("Subject", subjectId.Value);
    }
}
