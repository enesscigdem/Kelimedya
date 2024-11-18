namespace Paragraph.Core.Interfaces.Business
{
    public interface ICurrentUserService
    {
        public int GetUserId();
        public string GetEmail();
        public int GetCompanyId();
        bool GetIsAdmin();
    }
}