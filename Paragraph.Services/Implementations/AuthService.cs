using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Paragraph.Core.Enum;
using Paragraph.Core.IdentityEntities;
using Paragraph.Core.Models;
using Paragraph.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Paragraph.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<CustomUser> userManager,
                           IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Yeni kullanıcı kaydı işlemi.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<AuthResultViewModel> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new AuthResultViewModel
                {
                    Success = false,
                    Message = "Bu e-posta zaten kullanılıyor."
                };
            }

            var newUser = new CustomUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(newUser, dto.Password);

            if (!createResult.Succeeded)
            {
                return new AuthResultViewModel
                {
                    Success = false,
                    Message = "Kullanıcı oluşturulamadı.",
                    Errors = createResult.Errors.Select(e => e.Description).ToList()
                };
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, RoleNames.User);
            if (!roleResult.Succeeded)
            {
                return new AuthResultViewModel
                {
                    Success = false,
                    Message = "Kullanıcı oluşturuldu ancak rol atanamadı.",
                    Errors = roleResult.Errors.Select(e => e.Description).ToList()
                };
            }

            return new AuthResultViewModel
            {
                Success = true,
                Message = "Kayıt başarılı!"
            };
        }

        /// <summary>
        /// Kullanıcı girişi ve JWT token üretimi.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return null;

            var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordCheck)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles?.FirstOrDefault() ?? RoleNames.User;

            return GenerateJwtToken(user, role);
        }

        /// <summary>
        /// JWT oluşturur ve string olarak döner.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private string GenerateJwtToken(CustomUser user, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, role)
            };
            
            var identity = new ClaimsIdentity(claims, "Jwt");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
