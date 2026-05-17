using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.Aplicacion.DTOs
{
    public class ClasificacionCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
    }
}
