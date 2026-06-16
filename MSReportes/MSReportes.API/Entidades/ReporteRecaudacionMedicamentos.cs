namespace MSReportes.API.Entidades
{
    public class ReporteRecaudacionMedicamentos
    {
        public string Titulo { get; set; } = string.Empty;
        public string NombreEmpresa { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public string UsuarioGenerador { get; set; } = string.Empty;
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public List<ReporteRecaudacionMedicamentoDto> Detalle { get; set; } = new();
        public ResumenReporteRecaudacionMedicamentos Resumen { get; set; } = new();
        public string PiePagina { get; set; } = string.Empty;

        public string PeriodoTexto
        {
            get
            {
                if (Desde.HasValue && Hasta.HasValue)
                    return $"Del {Desde.Value:dd/MM/yyyy} al {Hasta.Value:dd/MM/yyyy}";

                if (Desde.HasValue)
                    return $"Desde {Desde.Value:dd/MM/yyyy}";

                if (Hasta.HasValue)
                    return $"Hasta {Hasta.Value:dd/MM/yyyy}";

                return "Todas las ventas confirmadas";
            }
        }
    }
}
