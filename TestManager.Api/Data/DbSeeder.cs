using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Models;

namespace TestManager.Api.Data;

/// <summary>Dữ liệu mẫu cho môi trường Development: 1 lớp học đầy đủ môn, học kỳ, giáo viên và 3 sinh viên.</summary>
public static class DbSeeder
{
    private const string SampleSubjectCode = "CS01";
    private const string SamplePassword = "123456";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> hasher, ILogger logger)
    {
        // Kiểm tra theo khoá tự nhiên (Subject.Code là unique) để khởi động lần 2 không seed trùng
        if (await db.Subjects.AnyAsync(s => s.Code == SampleSubjectCode))
        {
            logger.LogInformation("Seed: dữ liệu mẫu đã có (Subject {Code}), bỏ qua.", SampleSubjectCode);
            return;
        }

        User NewUser(string userName, string firstName, string middleName, string lastName)
        {
            var user = new User
            {
                Email = $"{userName}@example.com",
                UserName = userName,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName
            };
            user.PasswordHash = hasher.HashPassword(user, SamplePassword);
            return user;
        }

        var subject = new Subject { SubjectName = "Lập trình C#", Code = SampleSubjectCode };
        var semester = new Semester { Name = "Học kỳ 1 2026-2027", StartTime = new(2026, 9, 1), EndTime = new(2027, 1, 15) };
        var teacher = new Teacher
        {
            Name = "Nguyễn Thị Hoa", Status = true, Subject = subject,
            User = NewUser("hoant", "Nguyễn", "Thị", "Hoa")
        };
        Student[] students =
        [
            new() { Name = "Lê Văn An", Status = true, User = NewUser("anlv", "Lê", "Văn", "An") },
            new() { Name = "Phạm Thị Bình", Status = true, User = NewUser("binhpt", "Phạm", "Thị", "Bình") },
            new() { Name = "Hoàng Đức Cường", Status = true, User = NewUser("cuonghd", "Hoàng", "Đức", "Cường") }
        ];

        db.Classes.Add(new ClassRoom
        {
            Name = "C# - K1",
            StartTime = new(2026, 9, 5),
            EndTime = new(2026, 12, 20),
            Subject = subject,
            Semester = semester,
            Teacher = teacher,
            StudentClasses = students.Select(s => new StudentClass { Student = s }).ToList()
        });

        // Một lần SaveChanges = một transaction: hoặc có đủ cả lớp mẫu, hoặc không có gì
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: đã tạo lớp mẫu 'C# - K1' với {Count} sinh viên.", students.Length);
    }
}
