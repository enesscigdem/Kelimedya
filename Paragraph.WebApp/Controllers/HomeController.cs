using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Paragraph.Core.Entities;
using Paragraph.WebApp.Models;

namespace Paragraph.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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

    // Aileler İçin İletişim
        public IActionResult ContactFamilies()
        {
            return View(new ContactFamiliesModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactFamilies(ContactFamiliesModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Form submit öncesinde Subject ve Body alanlarını JavaScript ile dolduracağız.
            var client = _httpClientFactory.CreateClient("DefaultApi");

            // API Message modeline uyumlu gönderiyoruz. (Mapping, view'dan doldurulmuş olmalı.)
            var response = await client.PostAsJsonAsync("api/messages", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Mesajınız başarıyla gönderildi.";
                return RedirectToAction("ContactFamilies");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("ContactFamilies API Hatası: {Error}", errorContent);
                ModelState.AddModelError("", "Mesaj gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
                return View(model);
            }
        }

        // Okullar İçin İletişim
        public IActionResult ContactSchools()
        {
            return View(new ContactSchoolsModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactSchools(ContactSchoolsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("DefaultApi");
            var response = await client.PostAsJsonAsync("api/messages", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Mesajınız başarıyla gönderildi.";
                return RedirectToAction("ContactSchools");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("ContactSchools API Hatası: {Error}", errorContent);
                ModelState.AddModelError("", "Mesaj gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
                return View(model);
            }
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