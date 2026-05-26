using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Servicios;

namespace FrontendVCare.Pages.Account
{
    public class SeguridadModel : PageModel
    {
        private readonly AuthClient _authClient;

        public SeguridadModel(AuthClient authClient)
        {
            _authClient = authClient;
        }

        public string Usuario { get; set; } = string.Empty;
        public string? PasswordError { get; set; }

        public void OnGet()
        {
            try
            {
                Usuario = HttpContext.Session.GetString("UserName") ?? "Usuario";
                
                if (TempData["PasswordError"] != null)
                {
                    PasswordError = TempData["PasswordError"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                PasswordError = $"Error al cargar la página: {ex.Message}";
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
                    return RedirectToPage();
                }

                currentPassword = currentPassword?.Trim() ?? string.Empty;
                newPassword = newPassword?.Trim() ?? string.Empty;
                confirmPassword = confirmPassword?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    TempData["PasswordError"] = "La contraseña actual es obligatoria.";
                    return RedirectToPage();
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    TempData["PasswordError"] = "La nueva contraseña es obligatoria.";
                    return RedirectToPage();
                }

                if (newPassword != confirmPassword)
                {
                    TempData["PasswordError"] = "La contraseña y su confirmación no coinciden.";
                    return RedirectToPage();
                }

                if (currentPassword == newPassword)
                {
                    TempData["PasswordError"] = "La nueva contraseña debe ser diferente a la actual.";
                    return RedirectToPage();
                }

                var resultado = await _authClient.CambiarContrasenaAsync(token, currentPassword, newPassword, confirmPassword);
                
                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                    return RedirectToPage();
                }
                else
                {
                    TempData["PasswordError"] = resultado.Mensaje ?? "Error al cambiar la contraseña.";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                TempData["PasswordError"] = $"Error: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
