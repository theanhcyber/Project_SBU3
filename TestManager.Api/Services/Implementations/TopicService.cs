using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.DTOs;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public class TopicService(AppDbContext db, IMapper mapper) : BaseService<Topic, TopicDto>(db, mapper), ITopicService
{
    public async Task<List<TopicDto>> GetAllAsync(long? subjectId = null)
    {
        var query = Set.AsQueryable();
        if (subjectId is not null) query = query.Where(t => t.SubjectId == subjectId);
        return await ToDtoListAsync(query.OrderBy(t => t.Id));
    }

    public Task<TopicDto?> GetByIdAsync(long id) => GetDtoByIdAsync(id);

    public async Task<TopicDto> CreateAsync(TopicDto dto)
    {
        await ValidateAsync(dto);

        var topic = new Topic { Name = dto.Name, SubjectId = dto.SubjectId, TeacherId = dto.TeacherId };
        Set.Add(topic);
        await Db.SaveChangesAsync();
        return (await GetByIdAsync(topic.Id))!;
    }

    public async Task<TopicDto?> UpdateAsync(long id, TopicDto dto)
    {
        var topic = await Set.FindAsync(id);
        if (topic is null) return null;

        await ValidateAsync(dto);
        topic.Name = dto.Name;
        topic.SubjectId = dto.SubjectId;
        topic.TeacherId = dto.TeacherId;
        await Db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public Task<bool> DeleteAsync(long id) => DeleteByIdAsync(id);

    private async Task ValidateAsync(TopicDto dto)
    {
        if (!await Db.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
            throw AppException.NotExists("Subject", dto.SubjectId);
        if (!await Db.Teachers.AnyAsync(t => t.Id == dto.TeacherId))
            throw AppException.NotExists("Teacher", dto.TeacherId);
    }
}
