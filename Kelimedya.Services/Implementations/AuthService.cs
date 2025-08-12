using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Kelimedya.Core.Enum;
using Kelimedya.Core.IdentityEntities;
using Kelimedya.Core.Models;
using Kelimedya.Services.Interfaces;
using Kelimedya.Persistence;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Kelimedya.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly KelimedyaDbContext _context;

        public AuthService(UserManager<CustomUser> userManager,
                           IConfiguration configuration,
                           KelimedyaDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
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
                PhoneNumber = dto.PhoneNumber,
                ClassGrade = dto.ClassGrade,
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
        public async Task<LoginResultViewModel> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new LoginResultViewModel { Success = false, Message = "Geçersiz e-posta veya şifre." };

            var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordCheck)
                return new LoginResultViewModel { Success = false, Message = "Geçersiz e-posta veya şifre." };

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles?.FirstOrDefault() ?? RoleNames.User;

            if (role == RoleNames.Student)
            {
                var latestOrder = await _context.Orders
                    .Where(o => o.UserId == user.Id.ToString() && !o.IsDeleted && o.Status == OrderStatus.Tamamlandı)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync();

                if (latestOrder == null || latestOrder.OrderDate.AddMonths(6) < DateTime.UtcNow)
                {
                    return new LoginResultViewModel { Success = false, Message = "Erişim süreniz dolmuştur." };
                }
            }

            var token = GenerateJwtToken(user, role);
            return new LoginResultViewModel { Success = true, Token = token, Role = role };
        }

        public async Task<List<CustomUser>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return users;
        }

        public async Task<string?> GenerateTokenForUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
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
                new Claim( ClaimTypes.NameIdentifier, user.Id.ToString() ),
                new Claim( ClaimTypes.Name,           user.UserName     ),
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

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }
    }
}
