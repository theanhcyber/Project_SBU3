using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(
    IStudentService students, IStudentExamService studentExams, IStudentClassService studentClasses) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> GetAll() => await students.GetAllAsync();

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetById(long id) =>
        await students.GetByIdAsync(id) is { } student ? student : NotFound();

    /// <summary>Danh sách bài thi của sinh viên (thay cho GET /getlist-exam bản Java).</summary>
    /// <param name="id">Id sinh viên</param>
    /// <param name="isTaken">true: đã thi, false: chưa thi, bỏ trống: tất cả</param>
    [HttpGet("{id:long}/exams")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<StudentExamDto>>> GetExams(long id, [FromQuery] bool? isTaken)
    {
        if (await students.GetByIdAsync(id) is null) return NotFound();
        return await studentExams.GetAllAsync(studentId: id, isTaken: isTaken);
    }

    /// <summary>Các lớp sinh viên đang học.</summary>
    [HttpGet("{id:long}/classes")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<StudentClassDto>>> GetClasses(long id)
    {
        if (await students.GetByIdAsync(id) is null) return NotFound();
        return await studentClasses.GetAllAsync(studentId: id);
    }

    /// <summary>Tạo sinh viên kèm tài khoản đăng nhập. Mã sinh viên (ST001...) tự sinh.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentDto>> Create(StudentDto dto)
    {
        var student = await students.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentDto>> Update(long id, StudentDto dto) =>
        await students.UpdateAsync(id, dto) is { } student ? student : NotFound();

    /// <summary>Xoá sinh viên, tài khoản, lớp đã đăng ký và bài thi của sinh viên.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id) =>
        await students.DeleteAsync(id) ? NoContent() : NotFound();
}
