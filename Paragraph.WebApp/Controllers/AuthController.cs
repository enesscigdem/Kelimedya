using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Paragraph.Core.DTOs;

namespace Paragraph.WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController(HttpClient httpClient) : Controller
    {
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Tüm alanları doldurun." });

            var response = await httpClient.PostAsJsonAsync("api/auth/login", dto);

            if (!response.IsSuccessStatusCode)
                return Unauthorized(new { success = false, message = "Geçersiz giriş bilgileri." });

            var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();

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
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Tüm alanları doldurun." });

            var response = await httpClient.PostAsJsonAsync("api/auth/register", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorMessage });
            }

            return Json(new { success = true, message = "Kayıt başarılı! Lütfen giriş yapın." });
        }
    }
}
