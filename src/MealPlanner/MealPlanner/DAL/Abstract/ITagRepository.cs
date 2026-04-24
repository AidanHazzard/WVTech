using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface ITagRepository : IRepository<Tag>
{
    Task<List<string>> GetTagNamesAsync();
    Task<List<Tag>> GetTagsByPopularityAsync();
    Task<Tag?> FindByNameAsync(string name);
}
