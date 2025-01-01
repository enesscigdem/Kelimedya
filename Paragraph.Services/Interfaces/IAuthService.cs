using System.Threading.Tasks;
using Paragraph.Core.Models;

namespace Paragraph.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultViewModel> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
    }
}