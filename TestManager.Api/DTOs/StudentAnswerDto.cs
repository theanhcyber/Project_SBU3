using System.ComponentModel.DataAnnotations;

namespace TestManager.Api.DTOs;

public class StudentAnswerDto
{
    [MinLength(1)]
    public List<Answering> Answers { get; set; } = [];

    public class Answering
    {
        [Range(1, long.MaxValue)]
        public long QuestionId { get; set; }

        public List<long> ChosenAnswers { get; set; } = [];
    }
}
