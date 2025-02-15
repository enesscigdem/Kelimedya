using Microsoft.AspNetCore.Mvc;
using Paragraph.WebApp.Areas.Admin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Paragraph.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MessagesController : Controller
    {
        private readonly HttpClient _httpClient;
        public MessagesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }
        
        // GET: /Admin/Messages
        public async Task<IActionResult> Index()
        {
            var messages = await _httpClient.GetFromJsonAsync<List<MessageViewModel>>("api/messages");
            return View(messages);
        }

        // GET: /Admin/Messages/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var message = await _httpClient.GetFromJsonAsync<MessageViewModel>($"api/messages/{id}");
            if (message == null)
                return NotFound();
            return View(message);
        }
    }
}