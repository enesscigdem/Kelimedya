using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Paragraph.Core.DTOs;
using Paragraph.Core.IdentityEntities;
using Paragraph.Services.Interfaces;

namespace Paragraph.Services.Implementations
{
    public class AuthService(UserManager<CustomUser> userManager, IConfiguration configuration)
        : IAuthService
    {
        public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return new AuthResultDto { Success = false, Message = "Bu e-posta zaten kullanılıyor." };

            var newUser = new CustomUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Kullanıcı oluşturulamadı: " + string.Join(", ", result.Errors)
                };

            return new AuthResultDto { Success = true, Message = "Kayıt başarılı!" };
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                return null;

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(CustomUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"] ?? throw new ArgumentNullException("JWT:Key is not configured."));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = configuration["JWT:Issuer"],
                Audience = configuration["JWT:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
