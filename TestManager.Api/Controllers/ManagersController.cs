using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManagersController(IManagerService managers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ManagerDto>>> GetAll() => await managers.GetAllAsync();

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerDto>> GetById(long id) =>
        await managers.GetByIdAsync(id) is { } manager ? manager : NotFound();

    /// <summary>Tạo manager kèm tài khoản đăng nhập.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerDto>> Create(ManagerDto dto)
    {
        var manager = await managers.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = manager.Id }, manager);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerDto>> Update(long id, ManagerDto dto) =>
        await managers.UpdateAsync(id, dto) is { } manager ? manager : NotFound();

    /// <summary>Đảo trạng thái active / inactive (thay cho POST /change bản Java).</summary>
    [HttpPatch("{id:long}/status")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerDto>> ToggleStatus(long id) =>
        await managers.ToggleStatusAsync(id) is { } manager ? manager : NotFound();

    /// <summary>Xoá manager và tài khoản đi kèm.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id) =>
        await managers.DeleteAsync(id) ? NoContent() : NotFound();
}
