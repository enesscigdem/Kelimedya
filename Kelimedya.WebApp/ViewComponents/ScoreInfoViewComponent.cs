using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Interfaces.Business;
using Kelimedya.Services.Interfaces;
using Kelimedya.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kelimedya.WebApp.ViewComponents;

public class ScoreInfoViewComponent : ViewComponent
{
    private readonly IHttpClientFactory _factory;
    private readonly ICurrentUserService _user;

    public ScoreInfoViewComponent(IHttpClientFactory factory, ICurrentUserService user)
    {
        _factory = factory;
        _user = user;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var client = _factory.CreateClient("DefaultApi");
        var userId = _user.GetUserId();
        var info = await client.GetFromJsonAsync<ScoreInfoViewModel>($"api/gamestats/score/{userId}")
                   ?? new ScoreInfoViewModel();
        return View(info);
    }
}
