using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class AnswerDto
{
    // Khi sửa câu hỏi: có Id => sửa đáp án cũ; không có Id => thêm mới; đáp án cũ không gửi lên => xoá
    public long? Id { get; set; }

    [Required, StringLength(500)]
    public string AnswerText { get; set; } = string.Empty;

    // Đề thi phát cho sinh viên để trống (null) để ẩn đáp án đúng
    public bool? IsCorrect { get; set; }

    public long? QuestionId { get; set; }
}
