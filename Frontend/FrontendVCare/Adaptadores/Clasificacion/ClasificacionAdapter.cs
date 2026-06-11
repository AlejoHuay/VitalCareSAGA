using FrontendVCare.Dto.ClasificacionDtos;

namespace FrontendVCare.Adaptadores
{
    public class ClasificacionAdapter : AdapterJSON<ClasificacionDto>
    {
        public ClasificacionAdapter(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<List<ClasificacionDto>> GetAllAsync()
        {
            return GetListAsync("api/clasificaciones");
        }

        public Task<ClasificacionDto?> GetByIdAsync(int id)
        {
            return GetAsync($"api/clasificaciones/{id}");
        }

        public Task<(bool Success, string? Message)> CreateAsync(ClasificacionDto clasificacion)
        {
            return PostWithMessageAsync("api/clasificaciones", clasificacion);
        }

        public Task<(bool Success, string? Message)> UpdateAsync(ClasificacionDto clasificacion)
        {
            return PutWithMessageAsync($"api/clasificaciones/{clasificacion.Id}", clasificacion);
        }

        public Task<bool> DeleteAsync(int id, int idUsuario)
        {
            return DeleteAsync($"api/clasificaciones/{id}?idUsuario={idUsuario}");
        }
    }
}
