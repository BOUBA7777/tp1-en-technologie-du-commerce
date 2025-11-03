using System.Text.Json;
using TP1.Models;

namespace TP1.Services
{
    public class DummyJsonService : IDummyJsonService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DummyJsonService> _logger;
        private const string BaseUrl = "https://dummyjson.com";

        public DummyJsonService(HttpClient httpClient, ILogger<DummyJsonService> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _logger = logger;
        }

        public async Task<List<DummyJsonUser>> GetUsersAsync(int limit = 30)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users?limit={limit}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Erreur API Dummy JSON: {response.StatusCode}");
                    return new List<DummyJsonUser>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };
                
                var result = JsonSerializer.Deserialize<DummyJsonUsersResponse>(json, options);
                
                return result?.Users ?? new List<DummyJsonUser>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs depuis Dummy JSON");
                return new List<DummyJsonUser>();
            }
        }

        public async Task<DummyJsonUser?> GetUserByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Erreur API Dummy JSON: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };
                
                return JsonSerializer.Deserialize<DummyJsonUser>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération de l'utilisateur {id} depuis Dummy JSON");
                return null;
            }
        }
    }
}






