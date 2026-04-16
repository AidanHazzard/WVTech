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
        return await _dbset.OrderBy(t => t.Name).Select(t => t.Name).ToListAsync();
    }
}
