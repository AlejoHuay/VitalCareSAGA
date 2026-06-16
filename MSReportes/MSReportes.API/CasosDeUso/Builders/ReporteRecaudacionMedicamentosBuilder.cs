using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.Builders
{
    public class ReporteRecaudacionMedicamentosBuilder : IReporteRecaudacionMedicamentosBuilder
    {
        private readonly ReporteRecaudacionMedicamentos reporte = new();

        public IReporteRecaudacionMedicamentosBuilder ConEncabezado(string nombreEmpresa, string titulo)
        {
            reporte.NombreEmpresa = nombreEmpresa;
            reporte.Titulo = titulo;
            reporte.FechaGeneracion = DateTime.Now;
            return this;
        }

        public IReporteRecaudacionMedicamentosBuilder ConUsuarioGenerador(string usuarioGenerador)
        {
            reporte.UsuarioGenerador = usuarioGenerador;
            return this;
        }

        public IReporteRecaudacionMedicamentosBuilder ConPeriodo(DateTime? desde, DateTime? hasta)
        {
            reporte.Desde = desde;
            reporte.Hasta = hasta;
            return this;
        }

        public IReporteRecaudacionMedicamentosBuilder ConDetalle(IEnumerable<ReporteRecaudacionMedicamentoDto> detalle)
        {
            reporte.Detalle = detalle
                .OrderByDescending(item => item.TotalRecaudado)
                .ThenByDescending(item => item.CantidadVendida)
                .ThenBy(item => item.Medicamento)
                .ToList();
            return this;
        }

        public IReporteRecaudacionMedicamentosBuilder ConResumen()
        {
            reporte.Resumen = new ResumenReporteRecaudacionMedicamentos
            {
                TotalMedicamentos = reporte.Detalle.Count,
                TotalUnidadesVendidas = reporte.Detalle.Sum(item => item.CantidadVendida),
                TotalVentas = reporte.Detalle.Sum(item => item.CantidadVentas),
                TotalRecaudado = reporte.Detalle.Sum(item => item.TotalRecaudado)
            };
            return this;
        }

        public IReporteRecaudacionMedicamentosBuilder ConPiePagina(string piePagina)
        {
            reporte.PiePagina = piePagina;
            return this;
        }

        public ReporteRecaudacionMedicamentos Build()
        {
            return reporte;
        }
    }
}
