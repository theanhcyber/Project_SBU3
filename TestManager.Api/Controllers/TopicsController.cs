using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController(ITopicService topics) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TopicDto>>> GetAll([FromQuery] long? subjectId) =>
        await topics.GetAllAsync(subjectId);

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TopicDto>> GetById(long id) =>
        await topics.GetByIdAsync(id) is { } topic ? topic : NotFound();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TopicDto>> Create(TopicDto dto)
    {
        var topic = await topics.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = topic.Id }, topic);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TopicDto>> Update(long id, TopicDto dto) =>
        await topics.UpdateAsync(id, dto) is { } topic ? topic : NotFound();

    /// <summary>Xoá chủ đề. Bị chặn nếu chủ đề còn câu hỏi.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id) =>
        await topics.DeleteAsync(id) ? NoContent() : NotFound();
}
