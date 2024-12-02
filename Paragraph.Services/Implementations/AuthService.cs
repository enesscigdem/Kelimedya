using Microsoft.AspNetCore.Identity;
using Paragraph.Core.IdentityEntities;
using Paragraph.Services.Interfaces;

namespace Paragraph.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<CustomUser> _userManager;

        public AuthService(UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> RegisterAsync(CustomUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            return isPasswordValid ? user.Id.ToString() : null;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            // Reset token gönderimi burada yapılır.
            return true;
        }
    }
}