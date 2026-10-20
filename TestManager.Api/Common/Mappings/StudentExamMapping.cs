using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho StudentExamMapper bên Java
public class StudentExamMapping : Profile
{
    // Cần Include Student, Exam.
    // StudentName, ExamName, ExamStartTime, ExamEndTime được tự flatten từ Student.Name, Exam.Name, Exam.StartTime, Exam.EndTime
    public StudentExamMapping()
    {
        CreateMap<StudentExam, StudentExamDto>();
    }
}
