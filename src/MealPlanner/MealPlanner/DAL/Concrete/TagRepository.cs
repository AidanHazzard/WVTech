using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(MealPlannerDBContext context) : base(context)
    {
    }

    public async Task<List<string>> GetTagNamesAsync()
    {
        return await _dbset
            .OrderByDescending(t => t.Recipes.Count)
            .Take(10)
            .Select(t => t.Name)
            .ToListAsync();
    }
}
