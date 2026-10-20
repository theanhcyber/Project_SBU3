using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Data;
using TestManager.Api.Models;

namespace TestManager.Api.Services;

// Base cho mọi service: DbContext + IMapper dùng chung (cấu hình mapping ở Common/Mappings)
public abstract class BaseService(AppDbContext db, IMapper mapper)
{
    protected AppDbContext Db { get; } = db;
    protected IMapper Mapper { get; } = mapper;
}

// Base cho service CRUD của một entity: chỉ chứa phần query dùng chung (ProjectTo, lấy theo Id, xoá theo Id).
// Các method public (GetByIdAsync, DeleteAsync...) vẫn khai báo ở từng service và gọi xuống các helper này
public abstract class BaseService<TEntity, TDto>(AppDbContext db, IMapper mapper) : BaseService(db, mapper)
    where TEntity : class, IEntity
    where TDto : class
{
    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    // ProjectTo: AutoMapper đọc Profile (Common/Mappings) và tự sinh SELECT / JOIN đúng các cột DTO cần
    // => không cần viết Include, không kéo cả entity lên bộ nhớ
    protected IQueryable<TDto> Project(IQueryable<TEntity> query) =>
        query.ProjectTo<TDto>(Mapper.ConfigurationProvider);

    protected Task<List<TDto>> ToDtoListAsync(IQueryable<TEntity> query) => Project(query).ToListAsync();

    protected Task<TDto?> GetDtoByIdAsync(long id) =>
        Project(Set.Where(e => e.Id == id)).FirstOrDefaultAsync();

    protected async Task<bool> DeleteByIdAsync(long id) =>
        await Set.Where(e => e.Id == id).ExecuteDeleteAsync() > 0;
}
