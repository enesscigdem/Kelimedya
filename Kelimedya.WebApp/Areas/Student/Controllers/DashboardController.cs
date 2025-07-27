using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Student.Models;
using Kelimedya.WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kelimedya.WebApp.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.Student)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        public async Task<IActionResult> Index()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var viewModel = await _httpClient.GetFromJsonAsync<StudentDashboardViewModel>($"api/student/dashboard/{studentId}");

            try
            {
                var userInfo = await _httpClient.GetFromJsonAsync<UserInfoViewModel>($"api/users/{studentId}");
                ViewBag.FullName = userInfo?.FullName;
            }
            catch (HttpRequestException)
            {
                ViewBag.FullName = null;
            }

            return View(viewModel);
        }
    }
}