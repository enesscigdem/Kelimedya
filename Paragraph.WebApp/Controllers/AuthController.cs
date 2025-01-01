using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Paragraph.Core.Models;

namespace Paragraph.WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Lütfen tüm alanları eksiksiz doldurun." });
            }

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized(new { success = false, message = "Geçersiz giriş bilgileri." });
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseViewModel>();
            if (string.IsNullOrEmpty(tokenResponse?.Token))
            {
                return StatusCode(500, new { success = false, message = "Giriş sırasında bir sorun oluştu." });
            }

            Response.Cookies.Append("AuthToken", tokenResponse.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(2)
            });

            var redirectUrl = GetRedirectUrlByRole(tokenResponse.Role);

            return Ok(new { success = true, message = "Giriş başarılı!", redirectUrl });
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Lütfen tüm alanları eksiksiz doldurun." });
            }

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = errorMessage });
            }

            return Ok(new { success = true, message = "Kayıt başarılı! Lütfen giriş yapın." });
        }

        private string GetRedirectUrlByRole(string role)
        {
            return role switch
            {
                "Admin"   => "/Admin/Dashboard",
                "Teacher" => "/Teacher/Dashboard",
                "Student" => "/Student/Dashboard",
                "User"    => "/",
                _         => "/"
            };
        }
    }
}
