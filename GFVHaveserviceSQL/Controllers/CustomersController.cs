using GFVHaveserviceSQL.Data;
using GFVHaveserviceSQL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFVHaveserviceSQL.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.ToListAsync();
            var taskGroups = await _context.WorkTasks
                .Where(t => t.CustomerId != null && t.ScheduledDate != null)
                .GroupBy(t => t.CustomerId!.Value)
                .ToListAsync();

            foreach (var group in taskGroups)
            {
                var customer = customers.FirstOrDefault(c => c.Id == group.Key);
                if (customer != null)
                {
                    customer.LastVisited = group
                        .Where(t => t.ScheduledDate!.Value.Date <= DateTime.Today)
                        .OrderByDescending(t => t.ScheduledDate)
                        .Select(t => t.ScheduledDate!.Value.Date)
                        .FirstOrDefault();

                    customer.NextVisit = group
                        .Where(t => t.ScheduledDate!.Value.Date >= DateTime.Today)
                        .OrderBy(t => t.ScheduledDate)
                        .Select(t => t.ScheduledDate!.Value.Date)
                        .FirstOrDefault();
                }
            }

            await _context.SaveChangesAsync();
            return View(customers);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [Authorize(Roles = "Admin,Worker")]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var tasks = await _context.WorkTasks
                .Where(t => t.CustomerId == id)
                .OrderBy(t => t.ScheduledDate)
                .ToListAsync();
            ViewBag.Tasks = tasks;

            return View(customer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
