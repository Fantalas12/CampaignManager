using CampaignManager.DTO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CampaignManager.Web.Services
{
    public class CampaignHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CampaignHttpClientService> _logger;

        public CampaignHttpClientService(HttpClient httpClient, ILogger<CampaignHttpClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CampaignDTO?> GetCampaignByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/campaign/{id}");
            var campaignJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API Response: {Response}", campaignJson);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CampaignDTO>(campaignJson);
            }
            return null;
        }

        public async Task<bool> CreateCampaignAsync(CampaignDTO campaignDTO)
        {
            var response = await _httpClient.PostAsJsonAsync("api/campaign", campaignDTO);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCampaignAsync(Guid id, CampaignDTO campaignDTO)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/campaign/{id}", campaignDTO);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCampaignByIdAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/campaign/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<CampaignDTO>> GetOwnedCampaignsForUserByIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/campaign/owned/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<CampaignDTO>>() ?? Enumerable.Empty<CampaignDTO>();
            }
            return Enumerable.Empty<CampaignDTO>();
        }

        public async Task<IEnumerable<CampaignDTO>> GetCampaignsForUserByIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/campaign/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<CampaignDTO>>() ?? Enumerable.Empty<CampaignDTO>();
            }
            return Enumerable.Empty<CampaignDTO>();
        }

        public async Task<bool> IsReservedCampaignNameForUserAsync(string name, string userId)
        {
            var response = await _httpClient.GetAsync($"api/campaign/reserved?name={name}&userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            return false;
        }
    }
}
