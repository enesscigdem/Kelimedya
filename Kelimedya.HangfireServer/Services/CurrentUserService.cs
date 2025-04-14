using Kelimedya.Core;
using Kelimedya.Core.Interfaces.Business;

namespace Kelimedya.HangfireServer.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public int GetUserId()
        {
            return Constants.HANGFIRE_USER_ID;
        }
        public string GetEmail()
        {
            throw new NotImplementedException();
        }
        public int GetCompanyId()
        {
            throw new NotImplementedException();
        }
        public bool GetIsAdmin()
        {
            throw new NotImplementedException();
        }
    }
}