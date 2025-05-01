using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Interfaces.Business;
using Kelimedya.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kelimedya.WebApp.ViewComponents
{
    public class MiniCartViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _factory;
        private readonly ICurrentUserService _user;

        public MiniCartViewComponent(IHttpClientFactory factory, ICurrentUserService user)
        {
            _factory = factory;
            _user    = user;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _factory.CreateClient("DefaultApi");
            var userId = _user.GetUserId();
            var cart   = await client.GetFromJsonAsync<CartDto>($"api/cart/{userId}") 
                         ?? new CartDto();

            var badgeCount = cart.Items.Sum(i => i.Quantity);
            return View(badgeCount);
        }
    }
}