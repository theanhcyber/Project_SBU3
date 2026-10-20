using AutoMapper;
using TestManager.Api.DTOs;
using TestManager.Api.Models;

namespace TestManager.Api.Common.Mappings;

public class AnswerMapping : Profile
{
    public AnswerMapping()
    {
        CreateMap<Answer, AnswerDto>();
    }
}
