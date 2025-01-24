using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Paragraph.WebApp.Models;

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
    // Hakkımızda Alt Menüsü
    public IActionResult WhoAreWe()
    {
        return View();
    }

    public IActionResult Mission()
    {
        return View();
    }

    public IActionResult Vision()
    {
        return View();
    }

    // İletişim Alt Menüsü
    public IActionResult ContactFamilies()
    {
        return View();
    }

    public IActionResult ContactSchools()
    {
        return View();
    }

    // Öğrenci Profili (Bilgilerim)
    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public IActionResult Profile(UserProfileModel model)
    {
        if (ModelState.IsValid)
        {
            // Kullanıcı bilgilerini güncelleme işlemlerini gerçekleştirin
            // Örneğin:
            // _userService.UpdateUserProfile(User.Identity.Name, model);
            return RedirectToAction("Profile");
        }
        return View(model);
    }

    // Ödeme Sayfası
    [HttpGet]
    public IActionResult Payment()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Payment(PaymentModel model)
    {
        if (ModelState.IsValid)
        {
            // Ödeme işlemlerini gerçekleştirin
            // Örneğin:
            // _paymentService.ProcessPayment(model);
            return RedirectToAction("OrderConfirmation");
        }
        return View(model);
    }

    public IActionResult OrderConfirmation()
    {
        return View();
    }
}