using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Paragraph.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult BlogDetails()
    {
        return View();
    }

    public IActionResult Cart()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Product()
    {
        return View();
    }

    public IActionResult Products()
    {
        return View();
    }
}