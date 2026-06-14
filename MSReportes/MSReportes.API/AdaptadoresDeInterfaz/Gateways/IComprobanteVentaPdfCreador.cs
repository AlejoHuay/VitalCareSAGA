using MSReportes.API.Entidades;

namespace MSReportes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IComprobanteVentaPdfCreador
    {
        ArchivoReporteDto Crear(ComprobanteVentaDto comprobante);
    }
}
