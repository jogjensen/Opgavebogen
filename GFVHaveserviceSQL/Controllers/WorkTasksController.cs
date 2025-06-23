using GFVHaveserviceSQL.Data;
using GFVHaveserviceSQL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFVHaveserviceSQL.Controllers
{
    [Authorize]
    public class WorkTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkTasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? date, TimeSpan? time, string? workerId, bool showCompleted = false, string? sortOrder = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var query = _context.WorkTasks.Include(t => t.Customer).Include(t => t.AssignedWorkers).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(t => t.AssignedWorkers.Any(w => w.Id == user!.Id));
            }
            else
            {
                ViewBag.Workers = await _userManager.GetUsersInRoleAsync("Worker");
            }

            if (date.HasValue)
            {
                query = query.Where(t => t.ScheduledDate.HasValue && t.ScheduledDate.Value.Date == date.Value.Date);
            }

            if (time.HasValue)
            {
                query = query.Where(t => t.ScheduledDate.HasValue && t.ScheduledDate.Value.TimeOfDay == time.Value);
            }

            if (!string.IsNullOrEmpty(workerId))
            {
                query = query.Where(t => t.AssignedWorkers.Any(w => w.Id == workerId));
            }

            query = showCompleted
                ? query.Where(t => t.Status == WorkTaskStatus.Completed)
                : query.Where(t => t.Status != WorkTaskStatus.Completed);

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(t => t.Name);
                    break;
                case "date":
                    query = query.OrderBy(t => t.ScheduledDate);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(t => t.ScheduledDate);
                    break;
                case "status":
                    query = query.OrderBy(t => t.Status);
                    break;
                default:
                    query = query.OrderBy(t => t.Name);
                    break;
            }

            ViewBag.ShowCompleted = showCompleted;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");
            ViewBag.Time = time?.ToString(@"HH\:mm");
            ViewBag.SortOrder = sortOrder;

            return View(await query.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.Workers = await _userManager.GetUsersInRoleAsync("Worker");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkTask task, string[] workerIds, string[] recurrenceDays, string? scheduleDate, string? scheduleTime)
        {
            if (!string.IsNullOrEmpty(scheduleDate) && !string.IsNullOrEmpty(scheduleTime))
            {
                if (DateTime.TryParse($"{scheduleDate} {scheduleTime}", out var dt))
                {
                    task.ScheduledDate = dt;
                }
            }

            task.RecurrenceDays = string.Join(",", recurrenceDays ?? Array.Empty<string>());

            if (ModelState.IsValid)
            {
                var workers = await _userManager.Users.Where(u => workerIds.Contains(u.Id)).ToListAsync();
                var tasksToAdd = new List<WorkTask>();

                if (task.IsRecurring && task.ScheduledDate.HasValue && task.RecurrenceEndDate.HasValue && !string.IsNullOrEmpty(task.RecurrenceDays))
                {
                    var days = task.RecurrenceDays.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => Enum.Parse<DayOfWeek>(d));
                    var startDate = task.ScheduledDate.Value.Date;
                    var endDate = task.RecurrenceEndDate.Value.Date;
                    var time = task.ScheduledDate.Value.TimeOfDay;
                    for (var d = startDate; d <= endDate; d = d.AddDays(1))
                    {
                        if (days.Contains(d.DayOfWeek))
                        {
                            var newTask = new WorkTask
                            {
                                Name = task.Name,
                                TaskDescription = task.TaskDescription,
                                ScheduledDate = d + time,
                                CustomerId = task.CustomerId,
                                AssignedWorkers = workers.ToList(),
                                Status = workers.Any() ? WorkTaskStatus.Assigned : WorkTaskStatus.New,
                                IsRecurring = task.IsRecurring,
                                RecurrenceDays = task.RecurrenceDays,
                                RecurrenceEndDate = task.RecurrenceEndDate
                            };
                            tasksToAdd.Add(newTask);
                        }
                    }
                }
                else
                {
                    task.AssignedWorkers = workers;
                    task.Status = workers.Any() ? WorkTaskStatus.Assigned : WorkTaskStatus.New;
                    tasksToAdd.Add(task);
                }

                _context.AddRange(tasksToAdd);
                await _context.SaveChangesAsync();

                if (task.CustomerId.HasValue)
                {
                    await UpdateCustomerVisits(task.CustomerId.Value);
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.Workers = await _userManager.GetUsersInRoleAsync("Worker");
            return View(task);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.WorkTasks
                .Include(t => t.AssignedWorkers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.Workers = await _userManager.GetUsersInRoleAsync("Worker");
            return View(task);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkTask task, string[] workerIds, string[] recurrenceDays, string? scheduleDate, string? scheduleTime)
        {
            var existing = await _context.WorkTasks
                .Include(t => t.AssignedWorkers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existing == null)
                return NotFound();

            if (!string.IsNullOrEmpty(scheduleDate) && !string.IsNullOrEmpty(scheduleTime))
            {
                if (DateTime.TryParse($"{scheduleDate} {scheduleTime}", out var dt))
                {
                    existing.ScheduledDate = dt;
                }
                else
                {
                    existing.ScheduledDate = null;
                }
            }
            else
            {
                existing.ScheduledDate = null;
            }

            existing.RecurrenceDays = string.Join(",", recurrenceDays ?? Array.Empty<string>());

            if (ModelState.IsValid)
            {
                existing.Name = task.Name;
                existing.TaskDescription = task.TaskDescription;
                existing.CustomerId = task.CustomerId;
                existing.IsRecurring = task.IsRecurring;
                existing.RepeatEveryDays = task.RepeatEveryDays;
                existing.RecurrenceEndDate = task.RecurrenceEndDate;

                var workers = await _userManager.Users.Where(u => workerIds.Contains(u.Id)).ToListAsync();
                existing.AssignedWorkers = workers;
                if (existing.Status != WorkTaskStatus.Completed)
                {
                    existing.Status = workers.Any() ? WorkTaskStatus.Assigned : WorkTaskStatus.New;
                }

                await _context.SaveChangesAsync();

                if (existing.CustomerId.HasValue)
                {
                    await UpdateCustomerVisits(existing.CustomerId.Value);
                }

                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.Workers = await _userManager.GetUsersInRoleAsync("Worker");
            return View(task);
        }

        public async Task<IActionResult> Calendar(int? month, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            var query = _context.WorkTasks.Include(t => t.Customer).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(t => t.AssignedWorkers.Any(w => w.Id == user!.Id));
            }

            var currentMonth = new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1);
            int startDiff = ((int)currentMonth.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var calendarStart = currentMonth.AddDays(-startDiff);
            var nextMonth = currentMonth.AddMonths(1);
            int endDiff = ((int)nextMonth.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var calendarEnd = nextMonth.AddDays(7 - endDiff);

            var tasks = await query
                .Where(t => t.ScheduledDate >= calendarStart && t.ScheduledDate < calendarEnd)
                .ToListAsync();

            ViewBag.Month = currentMonth.Month;
            ViewBag.Year = currentMonth.Year;
            var prev = currentMonth.AddMonths(-1);
            var next = currentMonth.AddMonths(1);
            ViewBag.PrevMonth = prev.Month;
            ViewBag.PrevYear = prev.Year;
            ViewBag.NextMonth = next.Month;
            ViewBag.NextYear = next.Year;

            return View(tasks.OrderBy(t => t.ScheduledDate));
        }

        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.WorkTasks
                .Include(t => t.Customer)
                .Include(t => t.AssignedWorkers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }

        private async Task UpdateCustomerVisits(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return;

            var tasks = await _context.WorkTasks
                .Where(t => t.CustomerId == customerId && t.ScheduledDate != null)
                .ToListAsync();

            customer.LastVisited = tasks
                .Where(t => t.ScheduledDate!.Value.Date <= DateTime.Today)
                .OrderByDescending(t => t.ScheduledDate)
                .Select(t => t.ScheduledDate!.Value.Date)
                .FirstOrDefault();

            customer.NextVisit = tasks
                .Where(t => t.ScheduledDate!.Value.Date >= DateTime.Today)
                .OrderBy(t => t.ScheduledDate)
                .Select(t => t.ScheduledDate!.Value.Date)
                .FirstOrDefault();

            await _context.SaveChangesAsync();
        }
    }
}
