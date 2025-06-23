using GFVHaveserviceSQL.Data;
using GFVHaveserviceSQL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFVHaveserviceSQL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ContactSubmissionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactSubmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var submissions = await _context.ContactSubmissions
                .OrderByDescending(c => c.SubmittedOn)
                .ToListAsync();
            return View(submissions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkSeen(int id)
        {
            var submission = await _context.ContactSubmissions.FindAsync(id);
            if (submission != null && !submission.IsSeen)
            {
                submission.IsSeen = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
