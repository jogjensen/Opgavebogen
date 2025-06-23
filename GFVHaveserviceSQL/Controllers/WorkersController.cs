using GFVHaveserviceSQL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFVHaveserviceSQL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WorkersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var workers = await _userManager.GetUsersInRoleAsync("Worker");
            return View(workers);
        }

        public async Task<IActionResult> Details(string id, string? sortOrder)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worker = await _userManager.Users
                .Include(u => u.Tasks)
                    .ThenInclude(t => t.Customer)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (worker == null)
            {
                return NotFound();
            }
            IEnumerable<WorkTask> tasks = worker.Tasks;
            ViewBag.ScheduledSortParm = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";

            tasks = sortOrder switch
            {
                "name_desc" => tasks.OrderByDescending(t => t.Name),
                "name" => tasks.OrderBy(t => t.Name),
                "date_desc" => tasks.OrderByDescending(t => t.ScheduledDate),
                "date" => tasks.OrderBy(t => t.ScheduledDate),
                _ => tasks.OrderBy(t => t.Name)
            };

            worker.Tasks = tasks.ToList();

            return View(worker);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string name, string phone)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("email", "Email is required");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = phone,
                Name = name,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, "Worker123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Worker");
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worker = await _userManager.FindByIdAsync(id);
            if (worker == null)
            {
                return NotFound();
            }

            return View(worker);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string name, string email, string phone)
        {
            var worker = await _userManager.FindByIdAsync(id);
            if (worker == null)
            {
                return NotFound();
            }

            worker.Name = name;
            worker.Email = email;
            worker.UserName = email;
            worker.PhoneNumber = phone;
            var result = await _userManager.UpdateAsync(worker);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(worker);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worker = await _userManager.FindByIdAsync(id);
            if (worker == null)
            {
                return NotFound();
            }

            return View(worker);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var worker = await _userManager.FindByIdAsync(id);
            if (worker != null)
            {
                await _userManager.DeleteAsync(worker);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
