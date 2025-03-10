using Microsoft.AspNetCore.Mvc;
using Paragraph.Services.Interfaces;
using Paragraph.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Paragraph.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Kayıt olma işlemi.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(new { Message = result.Message, Errors = result.Errors });

            return Ok(new { Message = result.Message });
        }

        /// <summary>
        /// Giriş işlemi.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.LoginAsync(dto);

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { Message = "Geçersiz e-posta veya şifre." });

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var role = jwtToken?.Claims
                .FirstOrDefault(claim => claim.Type == "role")?.Value;

            return Ok(new
            {
                Token = token,
                Role = role
            });
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
