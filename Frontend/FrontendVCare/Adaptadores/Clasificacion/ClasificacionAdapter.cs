using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;

namespace FrontendVCare.Adaptadores
{
    public class ClasificacionAdapter
    {
        private readonly IAdapter<ClasificacionDto> _adapter;
        public ClasificacionAdapter(IAdapter<ClasificacionDto> adapter)
        {
            _adapter = adapter;
        }
        public Task<List<ClasificacionDto>> GetListAsync(string url)
        {
            return _adapter.GetListAsync(url);
        }

        public Task<ClasificacionDto?> GetAsync(string url)
        {
            return _adapter.GetAsync(url);
        }

        public Task<bool> PostAsync(string url, ClasificacionDto data)
        {
            return _adapter.PostAsync(url, data);
        }

        public Task<(bool Success, string? Message)> PostWithMessageAsync(string url, ClasificacionDto data)
        {
            return _adapter.PostWithMessageAsync(url, data);
        }

        public Task<bool> PutAsync(string url, ClasificacionDto data)
        {
            return _adapter.PutAsync(url, data);
        }

        public Task<(bool Success, string? Message)> PutWithMessageAsync(string url, ClasificacionDto data)
        {
            return _adapter.PutWithMessageAsync(url, data);
        }

        public Task<bool> DeleteAsync(string url)
        {
            return _adapter.DeleteAsync(url);
        }

        public Task<List<ClasificacionDto>> GetAllAsync()
        {
            return _adapter.GetListAsync($"api/clasificaciones");
        }

        public Task<ClasificacionDto?> GetByIdAsync(int id)
        {
            return _adapter.GetAsync($"api/clasificaciones/{id}");
        }

        public Task<(bool Success, string? Message)> CreateAsync(ClasificacionDto clasificacion)
        {
            return _adapter.PostWithMessageAsync($"api/clasificaciones", clasificacion);
        }

        public Task<(bool Success, string? Message)> UpdateAsync(ClasificacionDto clasificacion)
        {
            return _adapter.PutWithMessageAsync($"api/clasificaciones/{clasificacion.Id}", clasificacion);
        }

        public Task<bool> DeleteAsync(int id, int idUsuario)
        {
            return _adapter.DeleteAsync($"api/clasificaciones/{id}?idUsuario={idUsuario}");
        }
    }
}
