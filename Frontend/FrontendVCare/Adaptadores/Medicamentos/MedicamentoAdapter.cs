using FrontendVCare.Dto.MedicamentoDtos;

namespace FrontendVCare.Adaptadores
{
    public class MedicamentoAdapter
    {
        private readonly AdapterJSON<MedicamentoDto> _adapter;

        public MedicamentoAdapter(AdapterJSON<MedicamentoDto> adapter)
        {
            _adapter = adapter;
        }

        public Task<List<MedicamentoDto>> GetAllAsync()
        {
            return _adapter.GetListAsync("api/medicamentos");
        }

        public Task<MedicamentoDto?> GetByIdAsync(int id)
        {
            return _adapter.GetAsync($"api/medicamentos/{id}");
        }

        public Task<bool> CreateAsync(MedicamentoDto medicamento)
        {
            return _adapter.PostAsync("api/medicamentos", medicamento);
        }

        public Task<(bool Success, string? Message)> CreateWithMessageAsync(MedicamentoDto medicamento)
        {
            return _adapter.PostWithMessageAsync("api/medicamentos", medicamento);
        }

        public Task<bool> UpdateAsync(MedicamentoDto medicamento)
        {
            return _adapter.PutAsync($"api/medicamentos/{medicamento.Id}", medicamento);
        }

        public Task<(bool Success, string? Message)> UpdateWithMessageAsync(MedicamentoDto medicamento)
        {
            return _adapter.PutWithMessageAsync($"api/medicamentos/{medicamento.Id}", medicamento);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _adapter.DeleteAsync($"api/medicamentos/{id}");
        }

        public Task<bool> DeleteAsync(int id, int idUsuario)
        {
            return _adapter.DeleteAsync($"api/medicamentos/{id}?idUsuario={idUsuario}");
        }
    }
}
