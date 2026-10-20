using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

// Sinh viên đăng ký thi, nộp bài, giáo viên chấm điểm
[ApiController]
[Route("api/student-exams")]
public class StudentExamsController(IStudentExamService studentExams) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StudentExamDto>>> GetAll(
        [FromQuery] long? studentId, [FromQuery] long? examId, [FromQuery] bool? isTaken) =>
        await studentExams.GetAllAsync(studentId, examId, isTaken);

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentExamDto>> GetById(long id) =>
        await studentExams.GetByIdAsync(id) is { } studentExam ? studentExam : NotFound();

    /// <summary>Đăng ký thi (thay cho POST /save-Studentexam). Không được trùng bài hoặc trùng giờ thi.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentExamDto>> Register(StudentExamDto dto)
    {
        var studentExam = await studentExams.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = studentExam.Id }, studentExam);
    }

    /// <summary>Đăng ký nhiều bài thi (thay cho POST /savelist-exam). Lỗi một bài thì không lưu bài nào.</summary>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<List<StudentExamDto>>> RegisterMany(List<StudentExamDto> dtos) =>
        await studentExams.RegisterManyAsync(dtos);

    /// <summary>Đổi bài thi đã đăng ký (chỉ khi chưa làm bài).</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentExamDto>> Update(long id, StudentExamDto dto) =>
        await studentExams.UpdateAsync(id, dto) is { } studentExam ? studentExam : NotFound();

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id) =>
        await studentExams.DeleteAsync(id) ? NoContent() : NotFound();

    /// <summary>Sinh viên nộp bài (thay cho POST /savelist-answer).</summary>
    [HttpPost("{id:long}/submit")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentExamDto>> Submit(long id, StudentAnswerDto dto) =>
        await studentExams.SubmitAsync(id, dto) is { } studentExam ? studentExam : NotFound();

    /// <summary>Chấm điểm bài đã nộp (thay cho POST /save-mark).</summary>
    [HttpPost("{id:long}/mark")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentExamDto>> Mark(long id) =>
        await studentExams.MarkAsync(id) is { } studentExam ? studentExam : NotFound();
}
