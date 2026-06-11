using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MSReportes.API.FrameworksYDrivers.Creadores
{
    public class ReporteVentasPorRolPdfCreador : IReporteVentasPdfCreador
    {
        public ArchivoReporteDto Crear(ReporteVentasPorRol reporte)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            byte[] contenido = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(text => text.FontSize(11));

                    page.Header().Column(header =>
                    {
                        header.Item().Text(reporte.NombreEmpresa)
                            .FontSize(18)
                            .SemiBold()
                            .FontColor(Colors.Green.Darken2);

                        header.Item().Text(reporte.Titulo)
                            .FontSize(22)
                            .SemiBold()
                            .FontColor(Colors.Black);

                        header.Item().Text($"Fecha de generación: {reporte.FechaGeneracion:dd/MM/yyyy HH:mm:ss}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);

                        header.Item().Text($"Usuario generador: {reporte.UsuarioGenerador}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(25).Column(content =>
                    {
                        content.Spacing(15);

                        content.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .Background(Colors.Green.Lighten4)
                                .Padding(10)
                                .Column(col =>
                                {
                                    col.Item().Text("Total de ventas").SemiBold();
                                    col.Item().Text(reporte.Resumen.TotalVentas.ToString()).FontSize(16);
                                });

                            row.ConstantItem(20);

                            row.RelativeItem()
                                .Background(Colors.Blue.Lighten4)
                                .Padding(10)
                                .Column(col =>
                                {
                                    col.Item().Text("Total recaudado").SemiBold();
                                    col.Item().Text($"Bs. {reporte.Resumen.TotalRecaudado:N2}").FontSize(16);
                                });
                        });

                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(130);
                            });

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1)
                                    .Padding(6)
                                    .Text("Rol")
                                    .SemiBold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1)
                                    .Padding(6)
                                    .Text("Cantidad")
                                    .SemiBold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1)
                                    .Padding(6)
                                    .Text("Total Recaudado")
                                    .SemiBold();
                            });

                            foreach (var item in reporte.Detalle)
                            {
                                table.Cell().Border(1).Padding(6).Text(item.Rol);
                                table.Cell().Border(1).Padding(6).Text(item.CantidadVentas.ToString());
                                table.Cell().Border(1).Padding(6).Text($"Bs. {item.TotalRecaudado:N2}");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span(reporte.PiePagina);
                    });
                });
            }).GeneratePdf();

            return new ArchivoReporteDto
            {
                NombreArchivo = $"reporte-ventas-por-rol-{DateTime.Now:yyyyMMddHHmmss}.pdf",
                ContentType = "application/pdf",
                Contenido = contenido
            };
        }
    }
}