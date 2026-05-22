using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Helpers;
using FrontendVCare.Servicios;
using System.ComponentModel.DataAnnotations;

namespace FrontendVCare.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;
        private readonly AuthClient _authClient;

        public ProfileModel(UsuarioAdapter usuarioAdapter, AuthClient authClient)
        {
            _usuarioAdapter = usuarioAdapter;
            _authClient = authClient;
        }

        public string Usuario { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string RegistrationDate { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string CI { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string? MensajeError { get; set; }

        [BindProperty]
        public UsuarioActualizarDto UsuarioActualizar { get; set; } = new();

        [BindProperty]
        [RegularExpression(@"^$|^\d[A-Za-z]$", ErrorMessage = "El complemento del CI debe tener el formato 1A.")]
        public string CiComplemento { get; set; } = string.Empty;

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
                UserName = usuario.UserName;

                (string ciBase, string ciComplemento) = CiFormatoHelper.SepararCi(usuario.Ci);
                CiComplemento = ciComplemento;

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
                    Ci = ciBase,
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
                if (!ModelState.IsValid)
                {
                    await OnGetAsync();
                    return Page();
                }

                string ciBase = UsuarioActualizar.Ci;
                UsuarioActualizar.Ci = CiFormatoHelper.ConstruirCi(UsuarioActualizar.Ci, CiComplemento);

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
                    UsuarioActualizar.Ci = ciBase;
                    ModelState.AddModelError("", mensaje ?? "Error al actualizar el perfil.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                string? token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrWhiteSpace(token))
                {
                    TempData["PasswordError"] = "Sesión expirada. Por favor inicia sesión de nuevo.";
                    await OnGetAsync();
                    return Page();
                }

                currentPassword = currentPassword?.Trim() ?? string.Empty;
                newPassword = newPassword?.Trim() ?? string.Empty;
                confirmPassword = confirmPassword?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    TempData["PasswordError"] = "La contraseña actual es obligatoria.";
                    await OnGetAsync();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    TempData["PasswordError"] = "La nueva contraseña es obligatoria.";
                    await OnGetAsync();
                    return Page();
                }

                if (newPassword != confirmPassword)
                {
                    TempData["PasswordError"] = "La contraseña y su confirmación no coinciden.";
                    await OnGetAsync();
                    return Page();
                }

                var resultado = await _authClient.CambiarContrasenaAsync(token, currentPassword, newPassword, confirmPassword);
                
                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                    await OnGetAsync();
                    return RedirectToPage();
                }
                else
                {
                    TempData["PasswordError"] = resultado.Mensaje ?? "Error al cambiar la contraseña.";
                    await OnGetAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                TempData["PasswordError"] = $"Error: {ex.Message}";
                await OnGetAsync();
                return Page();
            }
        }
    }
}
