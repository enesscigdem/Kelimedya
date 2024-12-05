using System.Threading.Tasks;
using Paragraph.Core.DTOs;

namespace Paragraph.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
    }
}