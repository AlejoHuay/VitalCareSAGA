using System.Net.Http.Json;
using System.Text.Json;

namespace FrontendVCare.Adaptadores
{
    public class AdapterJSON<T>
    {
        private readonly HttpClient _httpClient;

        public AdapterJSON(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<T>> GetListAsync(string url)
        {
            return await _httpClient.GetFromJsonAsync<List<T>>(url) ?? new List<T>();
        }

        public async Task<T?> GetAsync(string url)
        {
            return await _httpClient.GetFromJsonAsync<T>(url);
        }

        public async Task<bool> PostAsync(string url, T data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? Message)> PostWithMessageAsync(string url, T data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            
            if (response.IsSuccessStatusCode)
                return (true, null);

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("mensaje", out var mensajeElement))
                    return (false, mensajeElement.GetString() ?? "Error desconocido");
            }
            catch { }

            return (false, "Error al procesar la solicitud");
        }

        public async Task<bool> PutAsync(string url, T data)
        {
            var response = await _httpClient.PutAsJsonAsync(url, data);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? Message)> PutWithMessageAsync(string url, T data)
        {
            var response = await _httpClient.PutAsJsonAsync(url, data);
            
            if (response.IsSuccessStatusCode)
                return (true, null);

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("mensaje", out var mensajeElement))
                    return (false, mensajeElement.GetString() ?? "Error desconocido");
            }
            catch { }

            return (false, "Error al procesar la solicitud");
        }

        public async Task<bool> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
    }
}
