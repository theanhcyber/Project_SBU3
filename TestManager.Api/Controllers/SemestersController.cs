using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SemestersController(ISemesterService semesters) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SemesterDto>>> GetAll() => await semesters.GetAllAsync();

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SemesterDto>> GetById(long id) =>
        await semesters.GetByIdAsync(id) is { } semester ? semester : NotFound();

    /// <summary>Tạo học kỳ. Ngày kết thúc phải ở tương lai và không trùng thời gian với học kỳ khác.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SemesterDto>> Create(SemesterDto dto)
    {
        var semester = await semesters.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = semester.Id }, semester);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SemesterDto>> Update(long id, SemesterDto dto) =>
        await semesters.UpdateAsync(id, dto) is { } semester ? semester : NotFound();

    /// <summary>Xoá học kỳ. Bị chặn nếu học kỳ còn lớp học.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id) =>
        await semesters.DeleteAsync(id) ? NoContent() : NotFound();
}
