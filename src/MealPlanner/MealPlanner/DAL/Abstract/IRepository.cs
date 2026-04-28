using System.Linq.Expressions;

namespace MealPlanner.DAL.Abstract;

public interface IRepository<TEntity> where TEntity : class, new()
{
    public TEntity? Read(int id);

    public List<TEntity> ReadAll();

    public TEntity CreateOrUpdate(TEntity entity);

    public void Delete(TEntity entity);

    public bool Exists(int id);

    public TEntity FindOrCreate(Expression<Func<TEntity, bool>> predicate, Func<TEntity> factory);
}