namespace MealPlanner.Models;

public class KrogerExport
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public List<KrogerExportItem> Items { get; set; } = [];
}
