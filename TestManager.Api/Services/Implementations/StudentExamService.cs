using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class StudentExamService(AppDbContext db, IMapper mapper) : BaseService<StudentExam, StudentExamDto>(db, mapper), IStudentExamService
{
    // Tương ứng StudentExamServiceImp.GetUnTakenExamImp (lọc theo sinh viên + đã thi / chưa thi)
    public async Task<List<StudentExamDto>> GetAllAsync(long? studentId = null, long? examId = null, bool? isTaken = null)
    {
        var query = Set.AsQueryable();
        if (studentId is not null) query = query.Where(se => se.StudentId == studentId);
        if (examId is not null) query = query.Where(se => se.ExamId == examId);
        if (isTaken is not null) query = query.Where(se => se.IsTaken == isTaken);
        return await ToDtoListAsync(query.OrderBy(se => se.Exam.StartTime));
    }

    public Task<StudentExamDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<StudentExamDto> RegisterAsync(StudentExamDto dto)
    {
        await ValidateRegistrationAsync(dto, excludeId: null);

        var studentExam = new StudentExam { StudentId = dto.StudentId, ExamId = dto.ExamId, IsTaken = false };
        Set.Add(studentExam);
        await Db.SaveChangesAsync();
        return (await GetByIdAsync(studentExam.Id))!;
    }

    // Tương ứng savelistStudentExam: đăng ký nhiều bài thi, lỗi một bài thì huỷ tất cả
    public async Task<List<StudentExamDto>> RegisterManyAsync(List<StudentExamDto> dtos)
    {
        await using var transaction = await Db.Database.BeginTransactionAsync();
        var result = new List<StudentExamDto>();
        for (var i = 0; i < dtos.Count; i++)
        {
            try { result.Add(await RegisterAsync(dtos[i])); }
            catch (AppException e) { throw new AppException($"Phần tử thứ {i + 1}: {e.Message}", e.StatusCode); }
        }
        await transaction.CommitAsync();
        return result;
    }

    // Đổi sang bài thi khác (chỉ khi chưa làm bài)
    public async Task<StudentExamDto?> UpdateAsync(long id, StudentExamDto dto)
    {
        var studentExam = await Set.FindAsync(id);
        if (studentExam is null) return null;
        if (studentExam.IsTaken)
            throw AppException.Conflict("Sinh viên đã làm bài, không thể đổi bài thi.");

        await ValidateRegistrationAsync(dto, excludeId: id);
        studentExam.StudentId = dto.StudentId;
        studentExam.ExamId = dto.ExamId;
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    // Tương ứng StudentServiceImp.saveStudentAnswer. Bài làm lưu dạng "qId:aId:aId|qId:aId"
    public async Task<StudentExamDto?> SubmitAsync(long id, StudentAnswerDto dto)
    {
        var studentExam = await Db.StudentExams
            .Include(se => se.Exam.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(q => q.Answers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(se => se.Id == id);
        if (studentExam is null) return null;
        if (studentExam.IsTaken)
            throw AppException.Conflict("Bài thi đã được nộp.");

        var examQuestions = studentExam.Exam.ExamQuestions.ToDictionary(
            eq => eq.QuestionId, eq => eq.Question.Answers.Select(a => a.Id).ToHashSet());

        var duplicated = dto.Answers.GroupBy(a => a.QuestionId).FirstOrDefault(g => g.Count() > 1)?.Key;
        if (duplicated is not null)
            throw new AppException($"Câu hỏi {duplicated} được trả lời nhiều lần.");

        var parts = new List<string>();
        foreach (var answering in dto.Answers)
        {
            if (!examQuestions.TryGetValue(answering.QuestionId, out var validAnswerIds))
                throw new AppException($"Câu hỏi {answering.QuestionId} không nằm trong đề thi.");
            var chosen = answering.ChosenAnswers.Distinct().ToList();
            var invalid = chosen.FirstOrDefault(a => !validAnswerIds.Contains(a));
            if (invalid != 0)
                throw new AppException($"Đáp án {invalid} không thuộc câu hỏi {answering.QuestionId}.");

            parts.Add(string.Join(":", chosen.Prepend(answering.QuestionId)));
        }

        studentExam.Answer = string.Join("|", parts);
        studentExam.IsTaken = true;
        studentExam.Mark = null;
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    /*
     * Chấm điểm, logic từ TeacherServiceImp.mark + studentMark:
     *   mark = totalScore / (tổng số đáp án đúng của cả đề) * (số đáp án đúng sinh viên chọn được)
     * Với mỗi câu: chỉ tính điểm nếu số đáp án sinh viên chọn >= số đáp án đúng của câu đó.
     */
    public async Task<StudentExamDto?> MarkAsync(long id)
    {
        var studentExam = await Db.StudentExams
            .Include(se => se.Exam.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(q => q.Answers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(se => se.Id == id);
        if (studentExam is null) return null;
        if (!studentExam.IsTaken || studentExam.Answer is null)
            throw new AppException("Sinh viên chưa nộp bài.");

        var correctAnswersByQuestion = studentExam.Exam.ExamQuestions.ToDictionary(
            eq => eq.QuestionId,
            eq => eq.Question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToHashSet());

        var totalCorrect = correctAnswersByQuestion.Values.Sum(s => s.Count);
        if (totalCorrect == 0)
            throw new AppException("Đề thi chưa có câu hỏi / đáp án đúng nên không chấm được.");

        var countTrueAnswer = 0;
        foreach (var part in studentExam.Answer.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            var ids = part.Split(':').Select(long.Parse).ToArray();
            if (!correctAnswersByQuestion.TryGetValue(ids[0], out var correctIds)) continue;

            var chosen = ids.Skip(1).ToArray();
            if (correctIds.Count <= chosen.Length)
                countTrueAnswer += chosen.Count(correctIds.Contains);
        }

        studentExam.Mark = (float)Math.Round(studentExam.Exam.TotalScore / totalCorrect * countTrueAnswer, 2);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // Logic từ StudentExamServiceImp.saveStudentExam: không đăng ký trùng bài thi hoặc trùng giờ thi
    private async Task ValidateRegistrationAsync(StudentExamDto dto, long? excludeId)
    {
        if (!await Db.Students.AnyAsync(s => s.Id == dto.StudentId))
            throw AppException.NotExists("Student", dto.StudentId);
        var exam = await Db.Exams.FindAsync(dto.ExamId) ?? throw AppException.NotExists("Exam", dto.ExamId);

        var conflict = await Db.StudentExams
            .Where(se => se.StudentId == dto.StudentId && se.Id != excludeId)
            .Where(se => se.ExamId == exam.Id ||
                         (exam.StartTime <= se.Exam.EndTime && exam.EndTime >= se.Exam.StartTime))
            .Select(se => new { se.ExamId, se.Exam.Name })
            .FirstOrDefaultAsync();
        if (conflict is not null)
            throw AppException.Conflict(conflict.ExamId == exam.Id
                ? $"Sinh viên đã đăng ký bài thi '{exam.Name}'."
                : $"Bài thi '{exam.Name}' trùng giờ với bài thi '{conflict.Name}' đã đăng ký.");
    }
}
