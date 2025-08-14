using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Kelimedya.Core.Models;
using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        [HttpGet, Route("giris-yap")]
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
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return Unauthorized(new { success = false, message = error?.Message ?? "Geçersiz giriş bilgileri." });
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseViewModel>();
            if (string.IsNullOrEmpty(tokenResponse?.Token))
            {
                return StatusCode(500, new { success = false, message = "Giriş sırasında bir sorun oluştu." });
            }

            Response.Cookies.Append("AuthToken", tokenResponse.Token, new CookieOptions
            {
                HttpOnly   = true,
                Secure     = false, 
                SameSite   = SameSiteMode.Lax,
                Path       = "/",    
                Expires    = DateTime.UtcNow.AddDays(2)
            });

            var redirectUrl = GetRedirectUrlByRole(tokenResponse.Role);

            return Ok(new { success = true, message = "Giriş başarılı!", redirectUrl });
        }

        [HttpGet, Route("kayit-ol")]
        public IActionResult Register(string? email)
        {
            ViewBag.PrefillEmail = email;
            return View();
        }


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
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");

            return RedirectToAction("Login", "Auth");
        }

        private string GetRedirectUrlByRole(string role)
        {
            return role switch
            {
                "Admin" => "/Admin/Dashboard",
                "Teacher" => "/Teacher/Dashboard",
                "Student" => "/Student/Dashboard",
                "User" => "/",
                _ => "/"
            };
        }
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _httpClient.GetAsync("api/auth/users");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { success = false, message = "Kullanıcıları alırken bir sorun oluştu." });
            }

            var users = await response.Content.ReadFromJsonAsync<List<UserViewModel>>();
            return Ok(new { success = true, users });
        }
    }

    public class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
