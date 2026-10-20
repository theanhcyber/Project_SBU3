using Microsoft.AspNetCore.Mvc;
using TestManager.Api.DTOs;
using TestManager.Api.Services;

namespace TestManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController(IQuestionService questions) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<QuestionDto>>> GetAll([FromQuery] long? topicId) =>
        await questions.GetAllAsync(topicId);

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionDto>> GetById(long id) =>
        await questions.GetByIdAsync(id) is { } question ? question : NotFound();

    /// <summary>Tạo câu hỏi kèm danh sách đáp án (ít nhất 2 đáp án, 1 đáp án đúng).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuestionDto>> Create(QuestionDto dto)
    {
        var question = await questions.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
    }

    /// <summary>Sửa câu hỏi. Đáp án có Id: sửa; không có Id: thêm mới; đáp án cũ không gửi lên: xoá.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionDto>> Update(long id, QuestionDto dto) =>
        await questions.UpdateAsync(id, dto) is { } question ? question : NotFound();

    /// <summary>Xoá câu hỏi và đáp án. Bị chặn nếu câu hỏi đang nằm trong đề thi.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id) =>
        await questions.DeleteAsync(id) ? NoContent() : NotFound();

    /// <summary>Import câu hỏi từ file Excel (thay cho POST /save-excel bản Java).</summary>
    /// <remarks>Tải file mẫu ở GET /api/questions/import-template.</remarks>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<QuestionDto>>> Import(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        return await questions.ImportExcelAsync(stream);
    }

    /// <summary>Tải file Excel mẫu để import câu hỏi.</summary>
    [HttpGet("import-template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public IActionResult DownloadTemplate() =>
        File(questions.BuildExcelTemplate(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "question-import-template.xlsx");
}
