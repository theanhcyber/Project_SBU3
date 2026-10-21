using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/student-classes")]
public class StudentClassesController(IStudentClassService studentClasses) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StudentClassDto>>> GetAll([FromQuery] long? classId, [FromQuery] long? studentId) =>
        await studentClasses.GetAllAsync(classId, studentId);

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentClassDto>> GetById(long id) =>
        await studentClasses.GetByIdAsync(id) is { } studentClass ? studentClass : NotFound();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentClassDto>> Create(StudentClassDto dto)
    {
        var studentClass = await studentClasses.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = studentClass.Id }, studentClass);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentClassDto>> Update(long id, StudentClassDto dto) =>
        await studentClasses.UpdateAsync(id, dto) is { } studentClass ? studentClass : NotFound();

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id) =>
        await studentClasses.DeleteAsync(id) ? NoContent() : NotFound();
}
