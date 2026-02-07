using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.Models;      
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MealPlanner.Controllers;
public class HomeController : Controller
{
    private readonly MealPlannerDBContext _context;
    public HomeController(MealPlannerDBContext context)
        {
            _context = context;
        }
    public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    u.Email
                })
                .ToListAsync();

            return View(users);
        }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
