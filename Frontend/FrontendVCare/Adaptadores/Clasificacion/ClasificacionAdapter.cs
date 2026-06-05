using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;

namespace FrontendVCare.Adaptadores
{
    public class ClasificacionAdapter
    {
        private readonly AdapterJSON<ClasificacionDto> _adapter;

        public ClasificacionAdapter(AdapterJSON<ClasificacionDto> adapter)
        {
            _adapter = adapter;
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
