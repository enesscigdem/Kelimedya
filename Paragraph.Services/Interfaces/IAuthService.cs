using System.Threading.Tasks;
using Paragraph.Core.Models;
using Paragraph.Core.IdentityEntities;
using System.Collections.Generic;

namespace Paragraph.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultViewModel> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<List<CustomUser>> GetAllUsersAsync();
    }
}