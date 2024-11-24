using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Paragraph.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            var result = await _accountService.RegisterAsync(username, email, password);
            if (result)
                return Ok(new { message = "Registration successful" });
            return BadRequest(new { message = "Registration failed" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var token = await _accountService.LoginAsync(email, password);
            if (!string.IsNullOrEmpty(token))
                return Ok(new { token });
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}