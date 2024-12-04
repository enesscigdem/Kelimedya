using System.Threading.Tasks;
using Paragraph.Core.DTOs;

namespace Paragraph.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(string email, string password);
    }
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}