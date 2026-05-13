using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IKrogerExportRepository
{
    Task SaveExportAsync(KrogerExport export);
    Task<List<KrogerExport>> GetExportsForUserAsync(string userId);
    Task<KrogerExport?> GetExportWithItemsAsync(int exportId, string userId);
}
