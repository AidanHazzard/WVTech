using System.Linq.Expressions;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;

namespace MealPlanner.DAL.Concrete;

public class IngredientBaseRepository : Repository<IngredientBase>, IIngredientBaseRepository
{
    public IngredientBaseRepository(MealPlannerDBContext context) : base(context) { }

    public override IngredientBase CreateOrUpdate(IngredientBase entity)
    {
        entity.Name = IngredientNameNormalizer.NormalizeKey(entity.Name);
        return base.CreateOrUpdate(entity);
    }

    public override IngredientBase FindOrCreate(Expression<Func<IngredientBase, bool>> predicate, Func<IngredientBase> factory)
    {
        return base.FindOrCreate(predicate, () =>
        {
            var entity = factory();
            entity.Name = IngredientNameNormalizer.NormalizeKey(entity.Name);
            return entity;
        });
    }

    public IngredientBase FindOrCreateByName(string name)
    {
        var normalized = IngredientNameNormalizer.NormalizeKey(name);
        return _dbset.FirstOrDefault(ib => ib.Name == normalized)
            ?? _dbset.Add(new IngredientBase { Name = normalized }).Entity;
    }
}
