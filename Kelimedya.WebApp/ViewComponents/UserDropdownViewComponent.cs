using System.Net.Http;
using System.Net.Http.Json;
using Kelimedya.Core.Interfaces.Business;
using Kelimedya.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kelimedya.WebApp.ViewComponents;

public class UserDropdownViewComponent : ViewComponent
{
    private readonly IHttpClientFactory _factory;
    private readonly ICurrentUserService _user;

    public UserDropdownViewComponent(IHttpClientFactory factory, ICurrentUserService user)
    {
        _factory = factory;
        _user = user;
    }

    public async Task<IViewComponentResult> InvokeAsync(string area)
    {
        var id = _user.GetUserId();
        if (id <= 0)
        {
            return View(new UserInfoViewModel { Area = area });
        }

        var client = _factory.CreateClient("DefaultApi");
        try
        {
            var info = await client.GetFromJsonAsync<UserInfoViewModel>($"api/users/{id}")
                       ?? new UserInfoViewModel();
            info.Area = area;
            return View(info);
        }
        catch (HttpRequestException)
        {
            return View(new UserInfoViewModel { Area = area });
        }
    }
}
