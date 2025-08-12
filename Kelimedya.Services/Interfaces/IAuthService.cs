using System.Threading.Tasks;
using Kelimedya.Core.Models;
using Kelimedya.Core.IdentityEntities;
using System.Collections.Generic;

namespace Kelimedya.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultViewModel> RegisterAsync(RegisterDto dto);
        Task<LoginResultViewModel> LoginAsync(LoginDto dto);
        Task<string?> GenerateTokenForUserAsync(string userId);
        Task<List<CustomUser>> GetAllUsersAsync();
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}