using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetPFE.Models;
using Microsoft.EntityFrameworkCore;

namespace MonProjetPFE.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) { _db = db; }
    public List<TaskItem> RecentTasks { get; set; } = new();

    public async Task OnGet()
    {
        RecentTasks = await _db.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(20).ToListAsync();
    }
}
