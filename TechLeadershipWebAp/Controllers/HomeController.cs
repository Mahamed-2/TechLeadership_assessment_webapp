using Microsoft.AspNetCore.Mvc;
using TechLeadershipWebApp.Services;

namespace TechLeadershipWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAssessmentService _assessmentService;

        public HomeController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Assessment");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}