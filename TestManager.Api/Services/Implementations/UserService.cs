using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class UserService(AppDbContext db, IMapper mapper, IPasswordHasher<User> passwordHasher) : BaseService<User, UserDto>(db, mapper), IUserService
{
    public async Task<List<UserDto>> GetAllAsync() =>
        await ToDtoListAsync(Set.OrderBy(u => u.Id));

    public Task<UserDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<bool> ChangePasswordAsync(long id, UserDto dto)
    {
        var user = await Set.FindAsync(id);
        if (user is null) return false;

        if (string.IsNullOrEmpty(dto.CurrentPassword) || string.IsNullOrEmpty(dto.Password))
            throw new AppException("Phải nhập currentPassword và password (mật khẩu mới).");
        var check = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (check == PasswordVerificationResult.Failed)
            throw new AppException("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
        await Db.SaveChangesAsync();
        return true;
    }

    public async Task<User> CreateUserAsync(UserDto dto)
    {
        ValidateProfile(dto);
        if (string.IsNullOrEmpty(dto.Password))
            throw new AppException("Phải nhập mật khẩu (user.password) khi tạo tài khoản.");
        await EnsureUniqueAsync(dto.Email!, dto.UserName!, excludeId: null);

        var user = new User
        {
            Email = dto.Email!,
            UserName = dto.UserName!,
            FirstName = dto.FirstName!,
            MiddleName = dto.MiddleName ?? string.Empty,
            LastName = dto.LastName!,
            Status = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
        return user;
    }

    public async Task UpdateProfileAsync(User user, UserDto dto)
    {
        ValidateProfile(dto);
        await EnsureUniqueAsync(dto.Email!, dto.UserName!, excludeId: user.Id);

        user.Email = dto.Email!;
        user.UserName = dto.UserName!;
        user.FirstName = dto.FirstName!;
        user.MiddleName = dto.MiddleName ?? string.Empty;
        user.LastName = dto.LastName!;
    }

    private static void ValidateProfile(UserDto dto)
    {
        var missing = new (string Name, string? Value)[]
            {
                ("email", dto.Email), ("userName", dto.UserName), ("firstName", dto.FirstName), ("lastName", dto.LastName)
            }
            .Where(f => string.IsNullOrWhiteSpace(f.Value))
            .Select(f => $"user.{f.Name}")
            .ToList();
        if (missing.Count > 0)
            throw new AppException($"Thiếu thông tin tài khoản: {string.Join(", ", missing)}.");
    }

    private async Task EnsureUniqueAsync(string email, string userName, long? excludeId)
    {
        if (await Set.AnyAsync(u => u.Email == email && u.Id != excludeId))
            throw AppException.Conflict($"Email '{email}' đã được sử dụng.");
        if (await Set.AnyAsync(u => u.UserName == userName && u.Id != excludeId))
            throw AppException.Conflict($"Username '{userName}' đã được sử dụng.");
    }
}
