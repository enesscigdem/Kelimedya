using System.Security.Claims;
using Kelimedya.Core.Enum;
using Kelimedya.Core.Interfaces.Business;

namespace Kelimedya.WebApp
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public int GetUserId()
        {
            return !_contextAccessor.HttpContext.User.Identity.IsAuthenticated
                ? -1
                : GetClaim(ClaimTypes.NameIdentifier);
        }

        public string GetEmail()
        {
            return GetStringClaim(ClaimTypes.Email);
        }

        public bool GetIsAdmin()
        {
            if (!CurrentUser().Identity.IsAuthenticated)
            {
                return false;
            }

            if (!CurrentUser().IsInRole(RoleNames.Admin))
            {
                return false;
            }

            return true;
        }

        private ClaimsPrincipal CurrentUser()
        {
            if (_contextAccessor?.HttpContext?.User == null)
            {
                throw new Exception("Claim not found");
            }

            return _contextAccessor?.HttpContext?.User;
        }

        private int GetClaim(string claimName)
        {
            var stringResult = CurrentUser().FindFirst(claimName).Value;
            if (int.TryParse(stringResult, out var result))
            {
                return result;
            }

            throw new Exception("Claim not found");
        }

        private string GetStringClaim(string claimName)
        {
            var result = CurrentUser().FindFirst(claimName).Value;
            if (string.IsNullOrEmpty(result))
            {
                return ""; //TODO: KURGU DETAYLI DÜŞÜNÜLMELİ, PROBLEM YARATABİLİR.
                throw new Exception("Claim not found");
            }

            return result;
        }


        private string GetStringClaimName(string claimName)
        {
            var result = CurrentUser().FindFirst(claimName).Value;
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Claim not found");
            }

            return result;
        }
    }
}