using Microsoft.EntityFrameworkCore;
using TestManager.Api.Models;

namespace TestManager.Api.Data;

// Mapping khớp với schema trong database/init.sql
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<ClassRoom> Classes => Set<ClassRoom>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
    public DbSet<StudentExam> StudentExams => Set<StudentExam>();

    // Mã tự sinh dạng PREFIX + 3 chữ số (thay cho @PostPersist bên Java)
    private static string CodeSql(string prefix) =>
        $"CONCAT(N'{prefix}', CASE WHEN Id < 1000 THEN RIGHT(CONCAT(N'00', Id), 3) ELSE CAST(Id AS NVARCHAR(20)) END)";

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.UserName).IsUnique();
        });

        b.Entity<Manager>(e =>
        {
            e.ToTable("Managers");
            e.HasIndex(m => m.Code).IsUnique();
            e.HasOne(m => m.User).WithOne(u => u.Manager)
                .HasForeignKey<Manager>(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Teacher>(e =>
        {
            e.ToTable("Teachers");
            e.Property(t => t.TeacherCode).HasComputedColumnSql(CodeSql("TC"), stored: true);
            e.HasOne(t => t.User).WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.Subject).WithMany(s => s.Teachers)
                .HasForeignKey(t => t.SubjectId).OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<Student>(e =>
        {
            e.ToTable("Students");
            e.Property(s => s.StudentCode).HasComputedColumnSql(CodeSql("ST"), stored: true);
            e.HasOne(s => s.User).WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Subject>(e =>
        {
            e.ToTable("Subjects");
            e.HasIndex(s => s.Code).IsUnique();
        });

        b.Entity<Semester>(e =>
        {
            e.ToTable("Semesters");
            e.Property(s => s.Code).HasComputedColumnSql(CodeSql("SE"), stored: true);
        });

        b.Entity<ClassRoom>(e =>
        {
            e.ToTable("Classes");
            e.Property(c => c.Code).HasComputedColumnSql(CodeSql("CL"), stored: true);
            e.HasOne(c => c.Subject).WithMany(s => s.ClassRooms)
                .HasForeignKey(c => c.SubjectId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(c => c.Semester).WithMany(s => s.ClassRooms)
                .HasForeignKey(c => c.SemesterId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(c => c.Teacher).WithMany(t => t.ClassRooms)
                .HasForeignKey(c => c.TeacherId).OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<StudentClass>(e =>
        {
            e.ToTable("StudentClasses");
            e.HasIndex(sc => new { sc.StudentId, sc.ClassId }).IsUnique();
            e.HasOne(sc => sc.ClassRoom).WithMany(c => c.StudentClasses)
                .HasForeignKey(sc => sc.ClassId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(sc => sc.Student).WithMany(s => s.StudentClasses)
                .HasForeignKey(sc => sc.StudentId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Topic>(e =>
        {
            e.ToTable("Topics");
            e.Property(t => t.Code).HasComputedColumnSql(CodeSql("TP"), stored: true);
            e.HasOne(t => t.Subject).WithMany(s => s.Topics)
                .HasForeignKey(t => t.SubjectId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.Teacher).WithMany(t => t.Topics)
                .HasForeignKey(t => t.TeacherId).OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<Question>(e =>
        {
            e.ToTable("Questions");
            e.HasOne(q => q.Topic).WithMany(t => t.Questions)
                .HasForeignKey(q => q.TopicId).OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<Answer>(e =>
        {
            e.ToTable("Answers");
            e.HasOne(a => a.Question).WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Exam>(e =>
        {
            e.ToTable("Exams");
            e.Property(x => x.Code).HasComputedColumnSql(CodeSql("EX"), stored: true);
            e.HasOne(x => x.Subject).WithMany(s => s.Exams)
                .HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Teacher).WithMany(t => t.Exams)
                .HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<ExamQuestion>(e =>
        {
            e.ToTable("ExamQuestions");
            e.HasIndex(eq => new { eq.ExamId, eq.QuestionId }).IsUnique();
            e.HasOne(eq => eq.Exam).WithMany(x => x.ExamQuestions)
                .HasForeignKey(eq => eq.ExamId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(eq => eq.Question).WithMany(q => q.ExamQuestions)
                .HasForeignKey(eq => eq.QuestionId).OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<StudentExam>(e =>
        {
            e.ToTable("StudentExams");
            e.HasIndex(se => new { se.StudentId, se.ExamId }).IsUnique();
            e.HasOne(se => se.Exam).WithMany(x => x.StudentExams)
                .HasForeignKey(se => se.ExamId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(se => se.Student).WithMany(s => s.StudentExams)
                .HasForeignKey(se => se.StudentId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
