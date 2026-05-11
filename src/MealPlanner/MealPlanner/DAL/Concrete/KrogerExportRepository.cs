using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class KrogerExportRepository : IKrogerExportRepository
{
    private readonly MealPlannerDBContext _context;

    public KrogerExportRepository(MealPlannerDBContext context)
    {
        _context = context;
    }

    public async Task SaveExportAsync(KrogerExport export)
    {
        _context.KrogerExports.Add(export);
        await _context.SaveChangesAsync();
    }

    public async Task<List<KrogerExport>> GetExportsForUserAsync(string userId)
    {
        return await _context.KrogerExports
            .Where(e => e.UserId == userId)
            .Include(e => e.Items)
            .OrderByDescending(e => e.ExportedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<KrogerExport?> GetExportWithItemsAsync(int exportId, string userId)
    {
        return await _context.KrogerExports
            .Include(e => e.Items)
            .FirstOrDefaultAsync(e => e.Id == exportId && e.UserId == userId);
    }
}
