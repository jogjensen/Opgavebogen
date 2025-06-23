using System.Diagnostics;
using GFVHaveserviceSQL.Models;
using GFVHaveserviceSQL.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace GFVHaveserviceSQL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactFormModel());
        }

        [HttpPost]
        public IActionResult Contact(ContactFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var submission = new ContactSubmission
                {
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    PlaceOfWork = model.PlaceOfWork,
                    Description = model.Description
                };
                _context.ContactSubmissions.Add(submission);
                _context.SaveChanges();

                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var smtpHost = config["Email:SmtpHost"];
                if (!string.IsNullOrEmpty(smtpHost))
                {
                    var smtpPort = int.TryParse(config["Email:SmtpPort"], out var port) ? port : 25;
                    var smtpUser = config["Email:SmtpUser"];
                    var smtpPass = config["Email:SmtpPass"];
                    var fromAddress = config["Email:From"] ?? smtpUser;

                    using var client = new SmtpClient(smtpHost, smtpPort)
                    {
                        EnableSsl = true,
                        Credentials = !string.IsNullOrEmpty(smtpUser) ? new NetworkCredential(smtpUser, smtpPass) : CredentialCache.DefaultNetworkCredentials
                    };

                    var mail = new MailMessage(fromAddress, "kontakt@gfv.nu")
                    {
                        Subject = "New contact request",
                        Body = $"Name: {model.Name}\nEmail: {model.Email}\nPhone: {model.Phone}\nPlace of work: {model.PlaceOfWork}\n\n{model.Description}"
                    };

                    client.Send(mail);
                }
                else
                {
                    _logger.LogInformation("No SMTP host configured; skipping email notification");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to process contact form submission");
            }

            return RedirectToAction("ContactConfirmation", "Home");

        }

        [HttpGet]
        public IActionResult ContactConfirmation()
        {
            return View();
        }

        [HttpGet("private")]
        public IActionResult Private()
        {
            return View();
        }

        [HttpGet("business")]
        public IActionResult Business()
        {
            return View();
        }

        [HttpGet("housing")]
        public IActionResult Housing()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
