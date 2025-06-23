using GFVHaveserviceSQL.Data;
using GFVHaveserviceSQL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFVHaveserviceSQL.Controllers
{
    [Authorize(Roles = "Worker")]
    public class WorkLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkLogsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int taskId)
        {
            var task = await _context.WorkTasks.FindAsync(taskId);
            if (task == null)
                return NotFound();
            ViewBag.Task = task;
            var now = DateTime.Now;
            now = now.AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond);
            return View(new WorkLog { WorkTaskId = taskId, StartTime = now, EndTime = now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkLog log)
        {
            var task = await _context.WorkTasks.FindAsync(log.WorkTaskId);
            if (task == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.WorkLogs.Add(log);
                task.TakenTime += log.DurationMinutes;
                task.Status = WorkTaskStatus.Completed;
                task.CompletedOn = log.EndTime;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "WorkTasks");
            }
            ViewBag.Task = task;
            return View(log);
        }
    }
}
