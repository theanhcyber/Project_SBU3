using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController(IExamService exams, IStudentExamService studentExams) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExamDto>>> GetAll([FromQuery] long? subjectId, [FromQuery] long? teacherId) =>
        await exams.GetAllAsync(subjectId, teacherId);

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExamDto>> GetById(long id) =>
        await exams.GetByIdAsync(id) is { } exam ? exam : NotFound();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExamDto>> Create(ExamDto dto)
    {
        var exam = await exams.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = exam.Id }, exam);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExamDto>> Update(long id, ExamDto dto) =>
        await exams.UpdateAsync(id, dto) is { } exam ? exam : NotFound();

    /// <summary>Xoá đề thi. Bị chặn nếu đã có sinh viên đăng ký.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id) =>
        await exams.DeleteAsync(id) ? NoContent() : NotFound();

    /// <summary>Đề thi kèm câu hỏi, đã ẩn đáp án đúng (thay cho GET /getlist-question bản Java).</summary>
    [HttpGet("{id:long}/questions")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExamDto>> GetPaper(long id) =>
        await exams.GetPaperAsync(id) is { } paper ? paper : NotFound();

    /// <summary>Thêm 1 câu hỏi vào đề (thay cho POST /save-examQuestion bản Java). Chỉ cần gửi questionId.</summary>
    [HttpPost("{id:long}/questions")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExamDto>> AddQuestion(long id, ExamQuestionDto dto) =>
        await exams.AddQuestionAsync(id, dto) is { } exam ? exam : NotFound();

    /// <summary>Gỡ một câu hỏi khỏi đề.</summary>
    [HttpDelete("{id:long}/questions/{questionId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveQuestion(long id, long questionId) =>
        await exams.RemoveQuestionAsync(id, questionId) ? NoContent() : NotFound();

    /// <summary>Danh sách sinh viên đăng ký đề thi này.</summary>
    [HttpGet("{id:long}/students")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<StudentExamDto>>> GetStudents(long id)
    {
        if (await exams.GetByIdAsync(id) is null) return NotFound();
        return await studentExams.GetAllAsync(examId: id);
    }
}
