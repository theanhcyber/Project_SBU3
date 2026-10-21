using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class StudentService(AppDbContext db, IMapper mapper, IUserService users) : BaseService<Student, StudentDto>(db, mapper), IStudentService
{
    public async Task<List<StudentDto>> GetAllAsync() =>
        await ToDtoListAsync(Set.OrderBy(s => s.Id));

    public Task<StudentDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<StudentDto> CreateAsync(StudentDto dto)
    {
        var student = new Student
        {
            Name = dto.Name,
            Status = true,
            User = await users.CreateUserAsync(dto.User)
        };
        Set.Add(student);
        await Db.SaveChangesAsync();
        return Mapper.Map<StudentDto>(student);
    }

    public async Task<StudentDto?> UpdateAsync(long id, StudentDto dto)
    {
        var student = await Set.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return null;

        await users.UpdateProfileAsync(student.User, dto.User);
        student.Name = dto.Name;
        student.Status = dto.Status ?? student.Status;
        await Db.SaveChangesAsync();
        return Mapper.Map<StudentDto>(student);
    }

    public async Task<bool> DeleteAsync(long id) =>
        await Db.Users.Where(u => u.Student != null && u.Student.Id == id).ExecuteDeleteAsync() > 0;
}
