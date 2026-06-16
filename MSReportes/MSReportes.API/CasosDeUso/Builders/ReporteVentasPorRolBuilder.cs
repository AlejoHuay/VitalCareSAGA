using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.Builders
{
    public class ReporteVentasPorRolBuilder : IReporteVentasPorRolBuilder
    {
        private readonly ReporteVentasPorRol _reporte = new();

        public IReporteVentasPorRolBuilder ConEncabezado(string nombreEmpresa, string titulo)
        {
            _reporte.NombreEmpresa = nombreEmpresa;
            _reporte.Titulo = titulo;
            _reporte.FechaGeneracion = DateTime.Now;

            return this;
        }

        public IReporteVentasPorRolBuilder ConUsuarioGenerador(string usuarioGenerador)
        {
            _reporte.UsuarioGenerador = usuarioGenerador;

            return this;
        }

        public IReporteVentasPorRolBuilder ConPeriodo(DateTime? desde, DateTime? hasta)
        {
            _reporte.Desde = desde;
            _reporte.Hasta = hasta;

            return this;
        }

        public IReporteVentasPorRolBuilder ConDetalle(IEnumerable<ReporteVentasPorRolDto> detalle)
        {
            _reporte.Detalle = detalle.ToList();

            return this;
        }

        public IReporteVentasPorRolBuilder ConResumen()
        {
            _reporte.Resumen = new ResumenReporteVentas
            {
                TotalVentas = _reporte.Detalle.Sum(x => x.CantidadVentas),
                TotalRecaudado = _reporte.Detalle.Sum(x => x.TotalRecaudado)
            };

            return this;
        }

        public IReporteVentasPorRolBuilder ConPiePagina(string piePagina)
        {
            _reporte.PiePagina = piePagina;

            return this;
        }

        public ReporteVentasPorRol Build()
        {
            if (string.IsNullOrWhiteSpace(_reporte.Titulo))
                throw new InvalidOperationException("El reporte debe tener un titulo.");

            return _reporte;
        }
    }
}
