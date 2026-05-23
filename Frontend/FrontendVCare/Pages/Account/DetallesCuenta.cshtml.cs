using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Servicios;

namespace FrontendVCare.Pages.Account
{
    public class DetallesCuentaModel : PageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        public DetallesCuentaModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        public string Usuario { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string CI { get; set; } = string.Empty;
        public string CiExtencion { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string? MensajeError { get; set; }

        [BindProperty]
        public UsuarioActualizarDto UsuarioActualizar { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Get user ID from session
                int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
                if (idUsuario == null || idUsuario <= 0)
                {
                    MensajeError = "No se pudo identificar al usuario.";
                    Usuario = HttpContext.Session.GetString("UserName") ?? "Usuario";
                    Role = HttpContext.Session.GetString("Role") ?? "Usuario";
                    return;
                }

                IdUsuario = idUsuario.Value;

                // Get user information from MSAuth service
                UsuarioDto? usuario = await _usuarioAdapter.ObtenerPorIdAsync(idUsuario.Value);
                
                if (usuario == null)
                {
                    MensajeError = "No se encontraron los datos del usuario.";
                    Usuario = HttpContext.Session.GetString("UserName") ?? "Usuario";
                    Role = HttpContext.Session.GetString("Role") ?? "Usuario";
                    return;
                }

                // Populate user properties
                Usuario = usuario.UserName;
                Role = usuario.Role;
                Email = usuario.Email;
                Phone = usuario.Telefono;
                Nombres = usuario.Nombres;
                ApellidoPaterno = usuario.ApellidoPaterno;
                ApellidoMaterno = usuario.ApellidoMaterno ?? string.Empty;
                CI = usuario.Ci;
                CiExtencion = usuario.CiExtencion ?? string.Empty;
                UserName = usuario.UserName;

                // Initialize update form with current values
                UsuarioActualizar = new UsuarioActualizarDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Email = usuario.Email,
                    Role = usuario.Role,
                    UserName = usuario.UserName,
                    Nombres = usuario.Nombres,
                    ApellidoPaterno = usuario.ApellidoPaterno,
                    ApellidoMaterno = usuario.ApellidoMaterno ?? string.Empty,
                    Ci = usuario.Ci,
                    CiExtencion = usuario.CiExtencion,
                    Telefono = usuario.Telefono
                };
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al obtener los datos del usuario: {ex.Message}";
                Usuario = HttpContext.Session.GetString("UserName") ?? "Usuario";
                Role = HttpContext.Session.GetString("Role") ?? "Usuario";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
                if (idUsuario == null || idUsuario <= 0)
                {
                    ModelState.AddModelError("", "No se pudo identificar al usuario.");
                    return Page();
                }

                UsuarioActualizar.IdUsuario = idUsuario.Value;

                var (exitoso, mensaje) = await _usuarioAdapter.ActualizarAsync(UsuarioActualizar);
                
                if (exitoso)
                {
                    // Refresh user data after successful update
                    await OnGetAsync();
                    TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError("", mensaje ?? "Error al actualizar el perfil.");
                    await OnGetAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }
}
