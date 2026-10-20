using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

// Thay cho Exammaper bên Java
public class ExamMapping : Profile
{
    // Cần Include Subject, Teacher, ExamQuestions. TeacherName được tự flatten từ Teacher.Name.
    // Questions chỉ điền khi xem đề (ExamService.GetPaperAsync)
    public ExamMapping()
    {
        CreateMap<Exam, ExamDto>()
            .ForMember(d => d.SubjectName, o => o.MapFrom(s => s.Subject != null ? s.Subject.SubjectName : null))
            .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.ExamQuestions.Count))
            .ForMember(d => d.Questions, o => o.Ignore());
    }
}
