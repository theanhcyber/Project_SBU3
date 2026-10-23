using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController(IClassRoomService classes, IStudentClassService studentClasses) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClassRoomDto>>> GetAll(
        [FromQuery] long? semesterId, [FromQuery] long? subjectId, [FromQuery] long? teacherId) =>
        await classes.GetAllAsync(semesterId, subjectId, teacherId);

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassRoomDto>> GetById(long id) =>
        await classes.GetByIdAsync(id) is { } classRoom ? classRoom : NotFound();

    /// <summary>Danh sách sinh viên trong lớp.</summary>
    [HttpGet("{id:long}/students")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<StudentClassDto>>> GetStudents(long id)
    {
        var students = await studentClasses.GetByClassCachedAsync(id);
        if (students.Count == 0 && await classes.GetByIdAsync(id) is null) return NotFound();
        return students;
    }

    /// <summary>Tạo lớp. Thời gian lớp phải nằm trong học kỳ và chưa kết thúc.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClassRoomDto>> Create(ClassRoomDto dto)
    {
        var classRoom = await classes.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = classRoom.Id }, classRoom);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassRoomDto>> Update(long id, ClassRoomDto dto) =>
        await classes.UpdateAsync(id, dto) is { } classRoom ? classRoom : NotFound();

    /// <summary>Xoá lớp và danh sách sinh viên trong lớp.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id) =>
        await classes.DeleteAsync(id) ? NoContent() : NotFound();
}
