using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Paragraph.Core.IdentityEntities;
using Paragraph.Services.Interfaces;
using System.Threading.Tasks;
using Paragraph.WebApp.Models;

namespace Paragraph.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;

        public AuthController(IAuthService authService, SignInManager<CustomUser> signInManager, UserManager<CustomUser> userManager)
        {
            _authService = authService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun.";
                return View(model);
            }

            var response = await _authService.RegisterAsync(new CustomUser
            {
                UserName = model.UserName,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }, model.Password);

            if (!response)
            {
                TempData["ErrorMessage"] = "Kayıt işlemi başarısız.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
                return Unauthorized("Geçersiz e-posta veya şifre.");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            return result.Succeeded ? Ok(new { UserId = user.Id, Message = "Giriş başarılı." }) : Unauthorized("Geçersiz e-posta veya şifre.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound("Bu e-posta adresine sahip kullanıcı bulunamadı.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Ok("Şifre sıfırlama bağlantısı e-posta ile gönderildi.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Çıkış başarılı.");
        }
    }
}
