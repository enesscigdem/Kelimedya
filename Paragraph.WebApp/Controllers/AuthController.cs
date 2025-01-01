using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Tüm alanları doldurun." });

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);

            if (!response.IsSuccessStatusCode)
                return Unauthorized(new { success = false, message = "Geçersiz giriş bilgileri." });

            var token = await response.Content.ReadFromJsonAsync<TokenResponseViewModel>();

            if (string.IsNullOrEmpty(token?.Token))
                return StatusCode(500, new { success = false, message = "Giriş sırasında bir sorun oluştu." });

            Response.Cookies.Append("AuthToken", token.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(2)
            });

            return Ok(new { success = true, message = "Giriş başarılı!" });
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Tüm alanları doldurun." });

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = errorMessage });
            }

            return Ok(new { success = true, message = "Kayıt başarılı! Lütfen giriş yapın." });
        }

    }
}
