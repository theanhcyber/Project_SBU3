using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class SubjectService(AppDbContext db, IMapper mapper) : BaseService<Subject, SubjectDto>(db, mapper), ISubjectService
{
    public async Task<List<SubjectDto>> GetAllAsync() =>
        await ToDtoListAsync(Set.OrderBy(s => s.Id));

    public Task<SubjectDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<SubjectDto> CreateAsync(SubjectDto dto)
    {
        await EnsureCodeAvailableAsync(dto.Code, excludeId: null);

        var subject = new Subject { SubjectName = dto.SubjectName, Code = dto.Code };
        Set.Add(subject);
        await Db.SaveChangesAsync();
        return Mapper.Map<SubjectDto>(subject);
    }

    public async Task<SubjectDto?> UpdateAsync(long id, SubjectDto dto)
    {
        var subject = await Set.FindAsync(id);
        if (subject is null) return null;

        await EnsureCodeAvailableAsync(dto.Code, excludeId: id);
        subject.SubjectName = dto.SubjectName;
        subject.Code = dto.Code;
        await Db.SaveChangesAsync();
        return Mapper.Map<SubjectDto>(subject);
    }

    // Môn học đang có lớp / chủ đề / đề thi / giáo viên sẽ bị chặn (409)
    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    private async Task EnsureCodeAvailableAsync(string code, long? excludeId)
    {
        if (await Set.AnyAsync(s => s.Code == code && s.Id != excludeId))
            throw AppException.Conflict($"Mã môn học '{code}' đã tồn tại.");
    }
}
