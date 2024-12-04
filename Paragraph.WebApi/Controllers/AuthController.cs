using Microsoft.AspNetCore.Mvc;
using Paragraph.Services.Interfaces;
using Paragraph.WebApp.Models;
using System.Threading.Tasks;
using Paragraph.Core;
using Paragraph.Core.DTOs;

namespace Paragraph.WebAPI.Controllers
{
    [Route(Routes.Auth.Base)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost(Routes.Auth.Register)]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new RegisterDto()
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                Name = model.Name,
                Surname = model.Surname
            };

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(new { Message = result.Message });

            return Ok(new { Message = result.Message });
        }

        [HttpPost(Routes.Auth.Login)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.LoginAsync(model.Email, model.Password);
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { Message = "Geçersiz e-posta veya şifre." });

            return Ok(new { Token = token });
        }
    }
}