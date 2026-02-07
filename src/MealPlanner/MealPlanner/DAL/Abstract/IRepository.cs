namespace MealPlanner.DAL.Abstract;

public interface IRepository<TEntity> where TEntity : class, new()
{
    public TEntity? Read(int id);

    public IQueryable<TEntity> ReadAll();

    public void CreateOrUpdate(TEntity entity);

    public void Delete(TEntity entity);

    public bool Exists(int id);
}