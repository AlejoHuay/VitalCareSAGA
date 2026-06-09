using ClosedXML.Excel;
using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;

namespace MSReportes.API.FrameworksYDrivers.Creadores
{
    public class ReporteVentasPorRolExcelCreador : IReporteVentasExcelCreador
    {
        public ArchivoReporteDto Crear(IEnumerable<ReporteVentasPorRolDto> datos)
        {
            var lista = datos.ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas por Rol");

            worksheet.Cell("A1").Value = "VITALCARE";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;

            worksheet.Cell("A2").Value = "REPORTE DE VENTAS POR ROL";
            worksheet.Cell("A2").Style.Font.Bold = true;
            worksheet.Cell("A2").Style.Font.FontSize = 14;

            worksheet.Cell("A3").Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

            worksheet.Cell("A5").Value = "Rol";
            worksheet.Cell("B5").Value = "Cantidad de Ventas";
            worksheet.Cell("C5").Value = "Total Recaudado Bs.";

            var headerRange = worksheet.Range("A5:C5");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int fila = 6;

            foreach (var item in lista)
            {
                worksheet.Cell(fila, 1).Value = item.Rol;
                worksheet.Cell(fila, 2).Value = item.CantidadVentas;
                worksheet.Cell(fila, 3).Value = item.TotalRecaudado;

                worksheet.Range(fila, 1, fila, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(fila, 1, fila, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                fila++;
            }

            worksheet.Cell(fila + 1, 1).Value = "TOTAL";
            worksheet.Cell(fila + 1, 1).Style.Font.Bold = true;

            worksheet.Cell(fila + 1, 2).FormulaA1 = $"SUM(B6:B{fila - 1})";
            worksheet.Cell(fila + 1, 2).Style.Font.Bold = true;

            worksheet.Cell(fila + 1, 3).FormulaA1 = $"SUM(C6:C{fila - 1})";
            worksheet.Cell(fila + 1, 3).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ArchivoReporteDto
            {
                NombreArchivo = $"reporte-ventas-por-rol-{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Contenido = stream.ToArray()
            };
        }
    }
}