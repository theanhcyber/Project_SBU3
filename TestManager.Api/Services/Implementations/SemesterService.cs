using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class SemesterService(AppDbContext db, IMapper mapper) : BaseService<Semester, SemesterDto>(db, mapper), ISemesterService
{
    public async Task<List<SemesterDto>> GetAllAsync() =>
        await ToDtoListAsync(Set.OrderBy(s => s.StartTime));

    public Task<SemesterDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<SemesterDto> CreateAsync(SemesterDto dto)
    {
        await ValidateAsync(dto, excludeId: null);

        var semester = new Semester { Name = dto.Name, StartTime = dto.StartTime, EndTime = dto.EndTime };
        Set.Add(semester);
        await Db.SaveChangesAsync();
        return Mapper.Map<SemesterDto>(semester);
    }

    public async Task<SemesterDto?> UpdateAsync(long id, SemesterDto dto)
    {
        var semester = await Set.FindAsync(id);
        if (semester is null) return null;

        await ValidateAsync(dto, excludeId: id);
        semester.Name = dto.Name;
        semester.StartTime = dto.StartTime;
        semester.EndTime = dto.EndTime;
        await Db.SaveChangesAsync();
        return Mapper.Map<SemesterDto>(semester);
    }

    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    private async Task ValidateAsync(SemesterDto dto, long? excludeId)
    {
        if (dto.StartTime >= dto.EndTime)
            throw new AppException("Ngày bắt đầu phải trước ngày kết thúc.");
        if (dto.EndTime < DateTime.Now)
            throw new AppException("Ngày kết thúc phải là hiện tại hoặc tương lai.");

        var overlapped = await Db.Semesters
            .Where(s => s.Id != excludeId && dto.StartTime <= s.EndTime && dto.EndTime >= s.StartTime)
            .Select(s => s.Name)
            .FirstOrDefaultAsync();
        if (overlapped is not null)
            throw AppException.Conflict($"Thời gian bị trùng với học kỳ '{overlapped}'.");
    }
}
