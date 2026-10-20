using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class ManagerService(AppDbContext db, IMapper mapper, IUserService users) : BaseService<Manager, ManagerDto>(db, mapper), IManagerService
{
    public async Task<List<ManagerDto>> GetAllAsync() =>
        await ToDtoListAsync(Set.OrderBy(m => m.Id));

    public Task<ManagerDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<ManagerDto> CreateAsync(ManagerDto dto)
    {
        await EnsureCodeAvailableAsync(dto.Code, excludeId: null);

        var manager = new Manager
        {
            Name = dto.Name,
            Code = dto.Code,
            Status = dto.Status ?? true,
            User = await users.CreateUserAsync(dto.User)
        };
        Set.Add(manager);
        await Db.SaveChangesAsync();
        return Mapper.Map<ManagerDto>(manager);
    }

    public async Task<ManagerDto?> UpdateAsync(long id, ManagerDto dto)
    {
        var manager = await Set.Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
        if (manager is null) return null;

        await EnsureCodeAvailableAsync(dto.Code, excludeId: id);
        await users.UpdateProfileAsync(manager.User, dto.User);
        manager.Name = dto.Name;
        manager.Code = dto.Code;
        manager.Status = dto.Status ?? manager.Status;
        await Db.SaveChangesAsync();
        return Mapper.Map<ManagerDto>(manager);
    }

    // Tương ứng ManagerServiceImp.changeStatus: đảo trạng thái active / inactive
    public async Task<ManagerDto?> ToggleStatusAsync(long id)
    {
        var manager = await Set.Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
        if (manager is null) return null;

        manager.Status = !manager.Status;
        await Db.SaveChangesAsync();
        return Mapper.Map<ManagerDto>(manager);
    }

    // Xoá User => Manager bị xoá theo (ON DELETE CASCADE)
    public async Task<bool> DeleteAsync(long id) =>
        await Db.Users.Where(u => u.Manager != null && u.Manager.Id == id).ExecuteDeleteAsync() > 0;

    private async Task EnsureCodeAvailableAsync(string code, long? excludeId)
    {
        if (await Set.AnyAsync(m => m.Code == code && m.Id != excludeId))
            throw AppException.Conflict($"Mã manager '{code}' đã tồn tại.");
    }
}
