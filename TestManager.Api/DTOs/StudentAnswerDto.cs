using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

// Bài làm sinh viên nộp lên (giống StudentAnswerDTO bên Java)
public class StudentAnswerDto
{
    [MinLength(1)]
    public List<Answering> Answers { get; set; } = [];

    public class Answering
    {
        [Range(1, long.MaxValue)]
        public long QuestionId { get; set; }

        // Các answerId sinh viên chọn
        public List<long> ChosenAnswers { get; set; } = [];
    }
}
