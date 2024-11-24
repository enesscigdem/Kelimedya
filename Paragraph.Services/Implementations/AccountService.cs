using Paragraph.Core.Interfaces;
using System.Threading.Tasks;

namespace Paragraph.Services.Implementations
{
    public class AccountService : IAccountService
    {
        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            // Kullanıcıyı veritabanına kaydetme işlemi (örnek)
            await Task.Delay(100); // Simüle edilmiş async işlem
            // Gerçek veritabanı kayıt işlemi buraya gelecek
            return true;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            await Task.Delay(100); // Simüle edilmiş async işlem
            // Örnek doğrulama işlemi
            if (email == "test@example.com" && password == "password123")
            {
                return "JWT-TOKEN-EXAMPLE";
            }
            return string.Empty;
        }
    }
}