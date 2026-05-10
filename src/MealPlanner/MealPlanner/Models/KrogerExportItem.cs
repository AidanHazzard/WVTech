namespace MealPlanner.Models;

public class KrogerExportItem
{
    public int Id { get; set; }
    public int ExportId { get; set; }
    public KrogerExport Export { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public float Amount { get; set; }
    public string Measurement { get; set; } = string.Empty;
    public string Upc { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
