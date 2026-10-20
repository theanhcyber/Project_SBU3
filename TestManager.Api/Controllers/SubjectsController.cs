using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController(ISubjectService subjects) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SubjectDto>>> GetAll() => await subjects.GetAllAsync();

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectDto>> GetById(long id) =>
        await subjects.GetByIdAsync(id) is { } subject ? subject : NotFound();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SubjectDto>> Create(SubjectDto dto)
    {
        var subject = await subjects.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SubjectDto>> Update(long id, SubjectDto dto) =>
        await subjects.UpdateAsync(id, dto) is { } subject ? subject : NotFound();

    /// <summary>Xoá môn học. Bị chặn nếu môn còn lớp / chủ đề / đề thi / giáo viên.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id) =>
        await subjects.DeleteAsync(id) ? NoContent() : NotFound();
}
