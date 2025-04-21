namespace Kelimedya.Core.Interfaces.Business
{
    public interface ICurrentUserService
    {
        public int GetUserId();
        public string GetEmail();
        bool GetIsAdmin();
    }
}