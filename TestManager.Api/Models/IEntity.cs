namespace TestManager.Api.Models;

// Entity nào cũng có khoá chính Id => BaseService<TEntity, TDto> viết được GetById / Delete dùng chung
public interface IEntity
{
    long Id { get; set; }
}
