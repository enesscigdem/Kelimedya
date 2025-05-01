using System.Collections.Generic;
using System.Linq;

namespace Kelimedya.WebAPI.Models
{
    public class ApplyCouponDto
    {
        public string UserId { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
    }
}