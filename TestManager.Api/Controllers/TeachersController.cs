using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController(ITeacherService teachers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TeacherDto>>> GetAll() => await teachers.GetAllAsync();

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeacherDto>> GetById(long id) =>
        await teachers.GetByIdAsync(id) is { } teacher ? teacher : NotFound();

    /// <summary>Tạo giáo viên kèm tài khoản đăng nhập. Mã giáo viên (TC001...) tự sinh.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeacherDto>> Create(TeacherDto dto)
    {
        var teacher = await teachers.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacher);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeacherDto>> Update(long id, TeacherDto dto) =>
        await teachers.UpdateAsync(id, dto) is { } teacher ? teacher : NotFound();

    /// <summary>Xoá giáo viên và tài khoản đi kèm. Bị chặn nếu giáo viên còn lớp / đề thi / chủ đề.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id) =>
        await teachers.DeleteAsync(id) ? NoContent() : NotFound();
}
