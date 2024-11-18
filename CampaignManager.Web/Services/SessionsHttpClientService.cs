using CampaignManager.DTO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CampaignManager.Web.Services
{
    public class SessionsHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SessionsHttpClientService> _logger;

        public SessionsHttpClientService(HttpClient httpClient, ILogger<SessionsHttpClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SessionDTO?> GetSessionByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/session/{id}");
            var sessionJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API Response: {Response}", sessionJson);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<SessionDTO>(sessionJson);
            }
            return null;
        }

        public async Task<bool> CreateSessionAsync(SessionDTO sessionDTO)
        {
            var response = await _httpClient.PostAsJsonAsync("api/session", sessionDTO);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSessionAsync(Guid id, SessionDTO sessionDTO)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/session/{id}", sessionDTO);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSessionByIdAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/session/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<SessionDTO>> GetSessionsForCampaignAsync(Guid campaignId)
        {
            var response = await _httpClient.GetAsync($"api/session/campaign/{campaignId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<SessionDTO>>() ?? Enumerable.Empty<SessionDTO>();
            }
            return Enumerable.Empty<SessionDTO>();
        }

        public async Task<(IEnumerable<SessionDTO> Sessions, int TotalCount)> GetPaginatedSessionsForCampaignAsync(Guid campaignId, int page, int pageSize)
        {
            var response = await _httpClient.GetAsync($"api/session/campaign/{campaignId}/paginated?page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var sessions = await response.Content.ReadFromJsonAsync<IEnumerable<SessionDTO>>();
                if (sessions != null)
                {
                    int totalCount = int.Parse(response.Headers.GetValues("X-Total-Count").FirstOrDefault() ?? "0");
                    return (sessions, totalCount);
                }
            }
            return (Enumerable.Empty<SessionDTO>(), 0);
        }

        public async Task<bool> IsReservedSessionNameForCampaignAsync(string name, Guid campaignId)
        {
            var response = await _httpClient.GetAsync($"api/session/reserved/name?name={name}&campaignId={campaignId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            return false;
        }

        public async Task<bool> IsReservedSessionDateForCampaignAsync(DateTime date, Guid campaignId)
        {
            var response = await _httpClient.GetAsync($"api/session/reserved/date?date={date.ToString("o")}&campaignId={campaignId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            return false;
        }
    }
}
