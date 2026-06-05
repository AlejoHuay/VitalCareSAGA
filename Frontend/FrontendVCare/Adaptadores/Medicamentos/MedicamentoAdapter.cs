using FrontendVCare.Dto;
using FrontendVCare.Servicios;
using FrontendVCare.Dto.MedicamentoDtos;

namespace FrontendVCare.Adaptadores
{
    public class MedicamentoAdapter
    {
        private readonly IAdapter<MedicamentoDto> _adapter;

        public MedicamentoAdapter(IAdapter<MedicamentoDto> adapter)
        {
            _adapter = adapter;
        }

        public Task<List<MedicamentoDto>> GetListAsync(string url)
        {
            return _adapter.GetListAsync(url);
        }
    }
}