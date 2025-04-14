using System.Threading.Tasks;
using Kelimedya.Core.Models;
using Kelimedya.Core.IdentityEntities;
using System.Collections.Generic;

namespace Kelimedya.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultViewModel> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<List<CustomUser>> GetAllUsersAsync();
    }
}