using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Paragraph.Services.HttpClients
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;

        protected BaseApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<TResponse> GetAsync<TResponse>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        protected async Task<bool> PostAsync<TRequest>(string url, TRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(url, request);
            return response.IsSuccessStatusCode;
        }
    }
}