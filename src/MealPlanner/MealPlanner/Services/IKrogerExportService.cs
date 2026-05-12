using MealPlanner.Models.DTO;

namespace MealPlanner.Services;

public interface IKrogerExportService
{
    Task<KrogerExportResult> RunExportAsync(string userId, string storeId, string krogerToken);
    Task<IEnumerable<KrogerExportSummaryDto>> GetExportHistoryAsync(string userId);
    Task<KrogerExportDetailDto?> GetExportDetailAsync(int exportId, string userId);
}
