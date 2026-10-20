using AutoMapper;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class QuestionService(AppDbContext db, IMapper mapper) : BaseService<Question, QuestionDto>(db, mapper), IQuestionService
{
    public async Task<List<QuestionDto>> GetAllAsync(long? topicId = null)
    {
        var query = Set.AsQueryable();
        if (topicId is not null) query = query.Where(q => q.TopicId == topicId);
        return await ToDtoListAsync(query.OrderBy(q => q.Id));
    }

    public Task<QuestionDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    // Bản Java gán isCorrect = true cho mọi đáp án; ở đây lấy đúng giá trị client gửi lên
    public async Task<QuestionDto> CreateAsync(QuestionDto dto)
    {
        ValidateAnswers(dto.Answers.Select(a => a.IsCorrect == true));
        if (!await Db.Topics.AnyAsync(t => t.Id == dto.TopicId))
            throw AppException.NotExists("Topic", dto.TopicId);

        var question = new Question
        {
            QuestionText = dto.QuestionText,
            Difficulty = dto.Difficulty,
            TopicId = dto.TopicId,
            Answers = dto.Answers.Select(a => new Answer { AnswerText = a.AnswerText, IsCorrect = a.IsCorrect == true }).ToList()
        };
        Set.Add(question);
        await Db.SaveChangesAsync();
        return (await GetByIdAsync(question.Id))!;
    }

    // Giữ nguyên Id của đáp án cũ (bài làm của sinh viên lưu theo answerId)
    public async Task<QuestionDto?> UpdateAsync(long id, QuestionDto dto)
    {
        var question = await Set.Include(q => q.Answers).FirstOrDefaultAsync(q => q.Id == id);
        if (question is null) return null;

        ValidateAnswers(dto.Answers.Select(a => a.IsCorrect == true));
        if (!await Db.Topics.AnyAsync(t => t.Id == dto.TopicId))
            throw AppException.NotExists("Topic", dto.TopicId);

        var existing = question.Answers.ToDictionary(a => a.Id);
        var unknownId = dto.Answers.FirstOrDefault(a => a.Id is not null && !existing.ContainsKey(a.Id.Value))?.Id;
        if (unknownId is not null)
            throw new AppException($"Đáp án {unknownId} không thuộc câu hỏi {id}.");

        var keptIds = dto.Answers.Where(a => a.Id is not null).Select(a => a.Id!.Value).ToHashSet();
        foreach (var removed in question.Answers.Where(a => !keptIds.Contains(a.Id)).ToList())
            Db.Answers.Remove(removed);

        foreach (var a in dto.Answers)
        {
            if (a.Id is { } answerId)
            {
                existing[answerId].AnswerText = a.AnswerText;
                existing[answerId].IsCorrect = a.IsCorrect == true;
            }
            else
            {
                question.Answers.Add(new Answer { AnswerText = a.AnswerText, IsCorrect = a.IsCorrect == true });
            }
        }

        question.QuestionText = dto.QuestionText;
        question.Difficulty = dto.Difficulty;
        question.TopicId = dto.TopicId;
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // Câu hỏi đang nằm trong đề thi sẽ bị chặn (409); đáp án bị xoá theo
    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    /*
     * Import câu hỏi từ Excel (logic từ TeacherServiceImp.addQuestionByExcelFile, bản Java chưa chạy được).
     * Dòng 1 là tiêu đề. Các cột:
     *   A: Mã môn | B: Mã chủ đề | C: Câu hỏi | D: Độ khó | E: Đáp án | F: Đúng ("x")
     * Dòng có cột C trống (hoặc ô merge) là đáp án tiếp theo của câu hỏi phía trên.
     * Toàn bộ file hợp lệ mới lưu, chỉ cần một dòng lỗi là không lưu gì.
     */
    public async Task<List<QuestionDto>> ImportExcelAsync(Stream excel)
    {
        XLWorkbook workbook;
        try { workbook = new XLWorkbook(excel); }
        catch (Exception) { throw new AppException("File không phải định dạng Excel (.xlsx) hợp lệ."); }

        using (workbook)
        {
            var topics = await Db.Topics.Include(t => t.Subject).ToDictionaryAsync(t => t.Code, StringComparer.OrdinalIgnoreCase);
            var questions = new List<Question>();
            Question? current = null;

            var sheet = workbook.Worksheet(1);
            foreach (var row in sheet.RowsUsed().Where(r => r.RowNumber() > 1))
            {
                var rowNo = row.RowNumber();
                string Cell(int col) => row.Cell(col).GetFormattedString().Trim();

                if (Cell(3) is { Length: > 0 } questionText)
                {
                    var topicCode = Cell(2);
                    if (!topics.TryGetValue(topicCode, out var topic))
                        throw new AppException($"Dòng {rowNo}: không tìm thấy chủ đề '{topicCode}'.");
                    var subjectCode = Cell(1);
                    if (subjectCode.Length > 0 && !subjectCode.Equals(topic.Subject.Code, StringComparison.OrdinalIgnoreCase))
                        throw new AppException($"Dòng {rowNo}: chủ đề '{topicCode}' không thuộc môn '{subjectCode}'.");

                    int? difficulty = null;
                    if (Cell(4) is { Length: > 0 } d)
                    {
                        if (!int.TryParse(d, out var value) || value is < 1 or > 5)
                            throw new AppException($"Dòng {rowNo}: độ khó phải là số từ 1 đến 5.");
                        difficulty = value;
                    }

                    current = new Question { QuestionText = questionText, Difficulty = difficulty, Topic = topic };
                    questions.Add(current);
                }
                else if (current is null)
                {
                    throw new AppException($"Dòng {rowNo}: đáp án không thuộc câu hỏi nào.");
                }

                if (Cell(5) is { Length: > 0 } answerText)
                {
                    var isCorrect = Cell(6).Equals("x", StringComparison.OrdinalIgnoreCase);
                    current.Answers.Add(new Answer { AnswerText = answerText, IsCorrect = isCorrect });
                }
            }

            if (questions.Count == 0)
                throw new AppException("File không có câu hỏi nào.");
            foreach (var q in questions)
            {
                try { ValidateAnswers(q.Answers.Select(a => a.IsCorrect)); }
                catch (AppException e) { throw new AppException($"Câu hỏi '{q.QuestionText}': {e.Message}"); }
            }

            Set.AddRange(questions);
            await Db.SaveChangesAsync();
            return Mapper.Map<List<QuestionDto>>(questions);
        }
    }

    public byte[] BuildExcelTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Questions");
        string[] header = ["Mã môn", "Mã chủ đề", "Câu hỏi", "Độ khó (1-5)", "Đáp án", "Đúng (x)"];
        for (var i = 0; i < header.Length; i++) sheet.Cell(1, i + 1).Value = header[i];
        sheet.Row(1).Style.Font.Bold = true;

        object[][] sample =
        [
            ["CS01", "TP001", "Từ khoá nào khai báo interface trong C#?", 1, "interface", "x"],
            ["", "", "", "", "class", ""],
            ["", "", "", "", "struct", ""],
            ["CS01", "TP002", "Phương thức LINQ nào trả về phần tử đầu tiên?", 2, "First", "x"],
            ["", "", "", "", "FirstOrDefault", "x"],
            ["", "", "", "", "Take", ""],
        ];
        for (var r = 0; r < sample.Length; r++)
            for (var c = 0; c < sample[r].Length; c++)
                sheet.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(sample[r][c]);

        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void ValidateAnswers(IEnumerable<bool> isCorrectFlags)
    {
        var flags = isCorrectFlags.ToList();
        if (flags.Count < 2)
            throw new AppException("Câu hỏi phải có ít nhất 2 đáp án.");
        if (!flags.Any(c => c))
            throw new AppException("Câu hỏi phải có ít nhất 1 đáp án đúng.");
    }
}
