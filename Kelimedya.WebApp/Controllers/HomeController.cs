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

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _currentUserService = currentUserService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet, Route("hakkimizda")]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet, Route("blog-detaylari")]
    public IActionResult BlogDetails()
    {
        return View();
    }

    [HttpGet, Route("sepet")]
    public async Task<IActionResult> Cart()
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var userId = _currentUserService.GetUserId();
        var cart = await client.GetFromJsonAsync<CartDto>($"api/cart/{userId}") 
                   ?? new CartDto();
        return View(cart);
    }

    [HttpPost, Route("sepete-kupon-uygula"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(string couponCode)
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var userId = _currentUserService.GetUserId();
        var resp = await client.PostAsJsonAsync("api/cart/applyCoupon", new {
            UserId = userId.ToString(),
            CouponCode = couponCode
        });

        if (resp.IsSuccessStatusCode)
            TempData["CouponSuccess"] = "Kupon başarıyla uygulandı!";
        else
            TempData["CouponError"] = await resp.Content.ReadAsStringAsync();

        return RedirectToAction("Cart");
    }

    [HttpPost, Route("sepette-kupon-kaldir"), ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCoupon()
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var userId = _currentUserService.GetUserId();
        await client.PostAsJsonAsync("api/cart/removeCoupon", new {
            UserId = userId,
            CouponCode = ""   // sadece UserId yeterli ama DTO bekliyor
        });
        TempData["CouponSuccess"] = "Kupon kaldırıldı.";
        return RedirectToAction("Cart");
    }

    [HttpPost, Route("sepete-ekle")]
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
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Cart");
        }
        else
        {
            TempData["Error"] = "Ürün sepetinize eklenemedi.";
            return RedirectToAction("Product", "Home", new { id = ProductId });
        }
    }

    [HttpPost, Route("sepet-miktar-guncelle")]
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

    [HttpPost, Route("sepet-urununu-kaldir")]
    public async Task<IActionResult> RemoveCartItem(int itemId)
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
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

    [HttpGet, Route("iletisim")]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpGet, Route("urunler")]
    public async Task<IActionResult> Products()
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var products = await client.GetFromJsonAsync<List<ProductViewModel>>("api/products");
        return View(products);
    }

    [HttpGet, Route("urun-detaylari/{id}")]
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

    [HttpGet, Route("biz-kimiz")]
    public IActionResult WhoAreWe()
    {
        return View();
    }

    [HttpGet, Route("misyon")]
    public IActionResult Mission()
    {
        return View();
    }

    [HttpGet, Route("vizyon")]
    public IActionResult Vision()
    {
        return View();
    }

    [HttpGet, Route("aileler-icin-iletisim")]
    public IActionResult ContactFamilies()
    {
        return View(new ContactFamiliesModel());
    }

    [HttpPost, ValidateAntiForgeryToken, Route("aileler-icin-iletisim")]
    public async Task<IActionResult> ContactFamilies(ContactFamiliesModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("DefaultApi");

        var response = await client.PostAsJsonAsync("api/messages", model);
        if (response.IsSuccessStatusCode)
        {
            TempData["ContactFamiliesSuccess"] = "Mesajınız başarıyla gönderildi.";
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

    [HttpGet, Route("okullar-icin-iletisim")]
    public IActionResult ContactSchools()
    {
        return View(new ContactSchoolsModel());
    }

    [HttpPost, ValidateAntiForgeryToken, Route("okullar-icin-iletisim")]
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
            TempData["ContactSchoolsSuccess"] = "Mesajınız başarıyla gönderildi.";
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


    [Authorize, HttpGet, Route("profilim")]
    public IActionResult Profile()
    {
        return View();
    }

    [Authorize, HttpPost, Route("profilim")]
    public IActionResult Profile(UserProfileModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Profile");
        }

        return View(model);
    }

    [HttpGet, Route("odeme")]
    public async Task<IActionResult> Payment()
    {
        var client = _httpClientFactory.CreateClient("DefaultApi");
        var userId = _currentUserService.GetUserId();

        var cart = await client.GetFromJsonAsync<CartDto>($"api/cart/{userId}")
                   ?? new CartDto();

        var model = new PaymentModel
        {
            Cart           = cart,
            CouponCode     = cart.CouponCode,
            DiscountAmount = cart.CouponDiscount
        };

        return View(model);
    }


    [HttpPost, Route("odeme")]
    public IActionResult Payment(PaymentModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("OrderConfirmation");
        }

        return View(model);
    }

    [HttpGet, Route("siparis-onay")]
    public IActionResult OrderConfirmation()
    {
        return View();
    }
}