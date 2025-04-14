using Microsoft.AspNetCore.Mvc;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RolesController : Controller
    {
        private readonly HttpClient _httpClient;
        public RolesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // GET: /Admin/Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _httpClient.GetFromJsonAsync<List<RoleViewModel>>("api/roles");
            return View(roles);
        }
    }
}