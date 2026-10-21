using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

public abstract class BaseService(AppDbContext db, IMapper mapper)
{
    protected AppDbContext Db { get; } = db;
    protected IMapper Mapper { get; } = mapper;
}

public abstract class BaseService<TEntity, TDto>(AppDbContext db, IMapper mapper) : BaseService(db, mapper)
    where TEntity : class, IEntity
    where TDto : class
{
    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    protected IQueryable<TDto> Project(IQueryable<TEntity> query) =>
        query.ProjectTo<TDto>(Mapper.ConfigurationProvider);

    protected Task<List<TDto>> ToDtoListAsync(IQueryable<TEntity> query) => Project(query).ToListAsync();

    protected Task<TDto?> GetDtoByIdAsync(long id) =>
        Project(Set.Where(e => e.Id == id)).FirstOrDefaultAsync();

    protected async Task<bool> DeleteByIdAsync(long id) =>
        await Set.Where(e => e.Id == id).ExecuteDeleteAsync() > 0;
}
