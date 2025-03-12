namespace FinalLab.Persistence.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAllAsync();
    IQueryable<TEntity> Query { get; }
    Task AddAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task SaveAsync();
}