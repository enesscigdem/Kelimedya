using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Paragraph.Core.Enum;
using Paragraph.Core.IdentityEntities;
using Paragraph.Core.Models;
using Paragraph.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Paragraph.Core.Entities;

namespace Paragraph.Services.Implementations
{
    public class WidgetService : IWidgetService
    {
        private readonly HttpClient _httpClient;
        public WidgetService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        public async Task<string> GetWidgetContentAsync(string key)
        {
            var widget = await _httpClient.GetFromJsonAsync<Widget>($"api/widgets/key/{key}");
            return widget?.Subject ?? "";
        }
    }
}
