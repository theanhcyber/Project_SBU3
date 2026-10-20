using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class ExamService(AppDbContext db, IMapper mapper) : BaseService<Exam, ExamDto>(db, mapper), IExamService
{
    public async Task<List<ExamDto>> GetAllAsync(long? subjectId = null, long? teacherId = null)
    {
        var query = Set.AsQueryable();
        if (subjectId is not null) query = query.Where(e => e.SubjectId == subjectId);
        if (teacherId is not null) query = query.Where(e => e.TeacherId == teacherId);
        return await ToDtoListAsync(query.OrderBy(e => e.StartTime));
    }

    public Task<ExamDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<ExamDto> CreateAsync(ExamDto dto)
    {
        await ValidateAsync(dto);

        var exam = new Exam();
        Apply(exam, dto);
        Set.Add(exam);
        await Db.SaveChangesAsync();
        return (await GetByIdAsync(exam.Id))!;
    }

    public async Task<ExamDto?> UpdateAsync(long id, ExamDto dto)
    {
        var exam = await Set.FindAsync(id);
        if (exam is null) return null;

        await ValidateAsync(dto);
        Apply(exam, dto);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // Đề đã có sinh viên đăng ký sẽ bị chặn (409); câu hỏi trong đề bị gỡ theo
    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    // Tương ứng ExamQuestionServiceImp.GetExam: lấy đề kèm danh sách câu hỏi
    public async Task<ExamDto?> GetPaperAsync(long id)
    {
        var exam = await Db.Exams
            .Include(e => e.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(q => q.Answers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exam is null) return null;

        var paper = Mapper.Map<ExamDto>(exam);
        paper.Questions = Mapper.Map<List<QuestionDto>>(exam.ExamQuestions.OrderBy(eq => eq.Id).Select(eq => eq.Question));
        // Ẩn đáp án đúng khi phát đề cho sinh viên
        foreach (var answer in paper.Questions.SelectMany(q => q.Answers))
            answer.IsCorrect = null;
        return paper;
    }

    // Tương ứng ExamQuestionServiceImp.saveExamQuestion: thêm 1 câu hỏi vào đề
    public async Task<ExamDto?> AddQuestionAsync(long id, ExamQuestionDto dto)
    {
        var exam = await Set.FindAsync(id);
        if (exam is null) return null;

        var question = await Db.Questions
            .Where(q => q.Id == dto.QuestionId)
            .Select(q => new { q.Id, q.Topic.SubjectId })
            .FirstOrDefaultAsync()
            ?? throw AppException.NotExists("Question", dto.QuestionId);
        if (question.SubjectId != exam.SubjectId)
            throw new AppException($"Câu hỏi {dto.QuestionId} không thuộc môn của đề thi.");
        if (await Db.ExamQuestions.AnyAsync(eq => eq.ExamId == id && eq.QuestionId == dto.QuestionId))
            throw AppException.Conflict($"Câu hỏi {dto.QuestionId} đã có trong đề.");

        Db.ExamQuestions.Add(new ExamQuestion { ExamId = id, QuestionId = dto.QuestionId });
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> RemoveQuestionAsync(long id, long questionId) =>
        await Db.ExamQuestions.Where(eq => eq.ExamId == id && eq.QuestionId == questionId).ExecuteDeleteAsync() > 0;

    private static void Apply(Exam exam, ExamDto dto)
    {
        exam.Name = dto.Name;
        exam.TotalScore = dto.TotalScore;
        exam.StartTime = dto.StartTime;
        exam.EndTime = dto.EndTime;
        exam.SubjectId = dto.SubjectId;
        exam.TeacherId = dto.TeacherId;
    }

    // Logic từ ExamServiceImp.saveExam
    private async Task ValidateAsync(ExamDto dto)
    {
        if (!await Db.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
            throw AppException.NotExists("Subject", dto.SubjectId);
        if (!await Db.Teachers.AnyAsync(t => t.Id == dto.TeacherId))
            throw AppException.NotExists("Teacher", dto.TeacherId);
        if (dto.StartTime >= dto.EndTime)
            throw new AppException("Thời gian bắt đầu phải trước thời gian kết thúc.");
    }
}
