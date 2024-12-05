using Microsoft.AspNetCore.Mvc;
using Paragraph.Services.Interfaces;
using Paragraph.Core.DTOs;
using System.Threading.Tasks;

namespace Paragraph.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(new { Message = result.Message });

            return Ok(new { Message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await authService.LoginAsync(dto);
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { Message = "Geçersiz e-posta veya şifre." });

            return Ok(new { Token = token });
        }
    }
}