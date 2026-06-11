using FrontendVCare.Dto.MedicamentoDtos;

namespace FrontendVCare.Adaptadores
{
    public class MedicamentoAdapter : AdapterJSON<MedicamentoDto>
    {
        public MedicamentoAdapter(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<List<MedicamentoDto>> GetAllAsync()
        {
            return GetListAsync("api/medicamentos");
        }

        public Task<MedicamentoDto?> GetByIdAsync(int id)
        {
            return GetAsync($"api/medicamentos/{id}");
        }

        public Task<bool> CreateAsync(MedicamentoDto medicamento)
        {
            return PostAsync("api/medicamentos", medicamento);
        }

        public Task<(bool Success, string? Message)> CreateWithMessageAsync(MedicamentoDto medicamento)
        {
            return PostWithMessageAsync("api/medicamentos", medicamento);
        }

        public Task<bool> UpdateAsync(MedicamentoDto medicamento)
        {
            return PutAsync($"api/medicamentos/{medicamento.Id}", medicamento);
        }

        public Task<(bool Success, string? Message)> UpdateWithMessageAsync(MedicamentoDto medicamento)
        {
            return PutWithMessageAsync($"api/medicamentos/{medicamento.Id}", medicamento);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return DeleteAsync($"api/medicamentos/{id}");
        }

        public Task<bool> DeleteAsync(int id, int idUsuario)
        {
            return DeleteAsync($"api/medicamentos/{id}?idUsuario={idUsuario}");
        }
    }
}
