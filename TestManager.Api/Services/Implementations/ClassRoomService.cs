using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class ClassRoomService(AppDbContext db, IMapper mapper) : BaseService<ClassRoom, ClassRoomDto>(db, mapper), IClassRoomService
{
    public async Task<List<ClassRoomDto>> GetAllAsync(long? semesterId = null, long? subjectId = null, long? teacherId = null)
    {
        var query = Set.AsQueryable();
        if (semesterId is not null) query = query.Where(c => c.SemesterId == semesterId);
        if (subjectId is not null) query = query.Where(c => c.SubjectId == subjectId);
        if (teacherId is not null) query = query.Where(c => c.TeacherId == teacherId);

        return await ToDtoListAsync(query.OrderBy(c => c.Id));
    }

    public Task<ClassRoomDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<ClassRoomDto> CreateAsync(ClassRoomDto dto)
    {
        await ValidateAsync(dto);

        var classRoom = new ClassRoom();
        Apply(classRoom, dto);
        Set.Add(classRoom);
        await Db.SaveChangesAsync();
        return (await GetByIdAsync(classRoom.Id))!;
    }

    public async Task<ClassRoomDto?> UpdateAsync(long id, ClassRoomDto dto)
    {
        var classRoom = await Set.FindAsync(id);
        if (classRoom is null) return null;

        await ValidateAsync(dto);
        Apply(classRoom, dto);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    private static void Apply(ClassRoom classRoom, ClassRoomDto dto)
    {
        classRoom.Name = dto.Name;
        classRoom.StartTime = dto.StartTime;
        classRoom.EndTime = dto.EndTime;
        classRoom.SubjectId = dto.SubjectId;
        classRoom.SemesterId = dto.SemesterId;
        classRoom.TeacherId = dto.TeacherId;
    }

    private async Task ValidateAsync(ClassRoomDto dto)
    {
        if (!await Db.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
            throw AppException.NotExists("Subject", dto.SubjectId);
        if (!await Db.Teachers.AnyAsync(t => t.Id == dto.TeacherId))
            throw AppException.NotExists("Teacher", dto.TeacherId);
        var semester = await Db.Semesters.FindAsync(dto.SemesterId)
            ?? throw AppException.NotExists("Semester", dto.SemesterId);

        if (dto.StartTime >= dto.EndTime)
            throw new AppException("Ngày bắt đầu phải trước ngày kết thúc.");
        if (dto.EndTime < DateTime.Now)
            throw new AppException("Ngày kết thúc phải là hiện tại hoặc tương lai.");
        if (dto.StartTime < semester.StartTime || dto.EndTime > semester.EndTime)
            throw new AppException(
                $"Thời gian lớp học phải nằm trong học kỳ '{semester.Name}' " +
                $"({semester.StartTime:dd/MM/yyyy} - {semester.EndTime:dd/MM/yyyy}).");
    }
}
