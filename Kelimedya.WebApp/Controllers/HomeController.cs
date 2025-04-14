using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Kelimedya.Core.Entities;
using Kelimedya.Core.Interfaces.Business;
using Kelimedya.WebApp.Areas.Admin.Models;
using Kelimedya.WebApp.Models;

namespace Kelimedya.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICurrentUserService _currentUserService;
    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _currentUserService = currentUserService;
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

    public async Task<IActionResult> Cart()
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var userId = _currentUserService.GetUserId();
        var cart = await client.GetFromJsonAsync<CartDto>($"api/cart/{userId}");
        if (cart == null)
        {
            cart = new CartDto();
        }
        return View(cart);
    }
    [HttpPost]
    public async Task<IActionResult> AddToCart(int ProductId)
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var userId = _currentUserService.GetUserId();

        var dto = new AddToCartDto
        {
            UserId = userId.ToString(),
            ProductId = ProductId,
            Quantity = 1
        };

        var response = await client.PostAsJsonAsync("api/cart/add", dto);
        if(response.IsSuccessStatusCode)
        {
            return RedirectToAction("Cart");
        }
        else
        {
            TempData["Error"] = "Ürün sepetinize eklenemedi.";
            return RedirectToAction("Product", "Home", new { id = ProductId });
        }
    }
    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(int itemId, int newQuantity)
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var response = await client.PostAsJsonAsync("/api/cart/updateQuantity", new UpdateCartItemDto
        {
            ItemId = itemId,
            Quantity = newQuantity
        });
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Miktar güncellenemedi.";
        }
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCartItem(int itemId)
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        // Artık API endpoint’imiz POST olarak çalışıyor.
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("itemId", itemId.ToString())
        });
        var response = await client.PostAsync("/api/cart/remove", formData);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Ürün sepetten kaldırılamadı.";
        }
        return RedirectToAction("Cart");
    }

    public IActionResult Contact()
    {
        return View();
    }

    public async Task<IActionResult> Products()
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var products = await client.GetFromJsonAsync<List<ProductViewModel>>("api/products");
        return View(products);
    }

    public async Task<IActionResult> Product(int id)
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var product = await client.GetFromJsonAsync<ProductViewModel>($"api/products/{id}");
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
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