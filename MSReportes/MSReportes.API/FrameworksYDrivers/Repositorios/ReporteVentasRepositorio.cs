using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;

namespace MSReportes.API.FrameworksYDrivers.Repositorios
{
    public class ReporteVentasRepositorio : IReporteVentasRepositorio
    {
        public async Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync()
        {
            // Aquí luego va la consulta real a BD o al MSVentas.
            await Task.CompletedTask;

            return new List<ReporteVentasPorRolDto>
            {
                new ReporteVentasPorRolDto
                {
                    Rol = "Administrador",
                    CantidadVentas = 10,
                    TotalRecaudado = 1500
                },
                new ReporteVentasPorRolDto
                {
                    Rol = "Vendedor",
                    CantidadVentas = 25,
                    TotalRecaudado = 3800
                }
            };
        }
    }
}