using System.Net.Http;
using System.Net.Http.Json;
using Kelimedya.Core.Interfaces.Business;
using Kelimedya.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kelimedya.WebApp.ViewComponents;

public class TeacherDropdownViewComponent : ViewComponent
{
    private readonly IHttpClientFactory _factory;
    private readonly ICurrentUserService _user;

    public TeacherDropdownViewComponent(IHttpClientFactory factory, ICurrentUserService user)
    {
        _factory = factory;
        _user = user;
    }

    public async Task<IViewComponentResult> InvokeAsync(string area)
    {
        var client = _factory.CreateClient("DefaultApi");
        var id = _user.GetUserId();
        var info = await client.GetFromJsonAsync<UserInfoViewModel>($"api/users/{id}")
                   ?? new UserInfoViewModel();
        info.Area = area;
        return View(info);
    }
}
