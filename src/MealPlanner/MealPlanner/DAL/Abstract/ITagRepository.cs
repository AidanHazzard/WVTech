using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface ITagRepository : IRepository<Tag>
{
    Task<List<string>> GetTagNamesAsync();
}
