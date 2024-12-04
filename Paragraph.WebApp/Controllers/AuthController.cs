using Microsoft.AspNetCore.Mvc;
using Paragraph.WebApp.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Paragraph.Core;

namespace Paragraph.WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public AuthController(HttpClient httpClient, IOptions<AppSettings> appSettings)
        {
            _httpClient = httpClient;
            _apiUrl = appSettings.Value.ApiUrl;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Tüm alanları doldurun." });

            var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}/api/auth/register", model);
            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "Kayıt işlemi başarısız oldu." });

            return Json(new { success = true, message = "Kayıt başarılı! Lütfen giriş yapın." });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Tüm alanları doldurun." });

            var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}/api/auth/login", model);
            if (!response.IsSuccessStatusCode)
                return Unauthorized(new { success = false, message = "Geçersiz giriş bilgileri." });

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (result == null || string.IsNullOrEmpty(result.Token))
                return StatusCode(500, new { success = false, message = "Giriş sırasında bir sorun oluştu." });

            Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(2)
            });

            return Ok(new { success = true, message = "Giriş başarılı!" });
        }

    }

    public class TokenResponse
    {
        public string Token { get; set; }
    }
}
