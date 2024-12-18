﻿using CampaignManager.DTO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CampaignManager.Web.Services
{
    public class NotesHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotesHttpClientService> _logger;

        public NotesHttpClientService(HttpClient httpClient, ILogger<NotesHttpClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /*
        public async Task<NoteDTO?> GetNoteByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/Notes/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NoteDTO>();
            }
            return null;
        } */

        public async Task<NoteDTO?> GetNoteByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/notes/{id}");
            var noteJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API Response: {Response}", noteJson);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<NoteDTO>(noteJson);
            }
            return null;
        }


        public async Task<bool> CreateNoteAsync(NoteDTO noteDTO)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Notes", noteDTO);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateNoteAsync(Guid id, NoteDTO noteDTO)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Notes/{id}", noteDTO);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteNoteByIdAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Notes/{id}");
            return response.IsSuccessStatusCode;
        }


        public async Task<(IEnumerable<NoteDTO> Notes, int TotalCount)> GetPaginatedNotesForSessionAsync(int sessionId, int page, int pageSize)
        {
            var response = await _httpClient.GetAsync($"api/Notes/session/{sessionId}/notes?page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var notes = await response.Content.ReadFromJsonAsync<IEnumerable<NoteDTO>>();
                if (notes != null)
                {
                    int totalCount = int.Parse(response.Headers.GetValues("X-Total-Count").FirstOrDefault() ?? "0");
                    return (notes, totalCount);
                }
            }
            return (Enumerable.Empty<NoteDTO>(), 0);
        }



    }
}
