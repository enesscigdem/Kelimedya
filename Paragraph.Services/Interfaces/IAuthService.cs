using System.Threading.Tasks;
using Paragraph.Core.IdentityEntities;

namespace Paragraph.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(CustomUser user, string password);
        Task<string> LoginAsync(string email, string password);
        Task<bool> ForgotPasswordAsync(string email);
    }
}